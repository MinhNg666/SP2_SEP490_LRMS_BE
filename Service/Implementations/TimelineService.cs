using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using Domain.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Implementations;

public class TimelineService : ITimelineService
{
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;
    private readonly INotificationService _notificationService;

    public TimelineService(IMapper mapper, LRMSDbContext context, INotificationService notificationService)
    {
        _mapper = mapper;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<TimelineResponse> CreateTimeline(TimelineRequest request, int createdBy)
    {
        try
        {
            if (request.StartDate == null || request.EndDate == null)
            {
                throw new ServiceException("StartDate and EndDate are required");
            }
            
            // Validate that no other active timeline with the same type exists
            if (request.TimelineType.HasValue)
            {
                var existingTimeline = await _context.Timelines
                    .Where(t => t.TimelineType == request.TimelineType &&
                           t.Status == (int)TimelineStatusEnum.Active)
                    .FirstOrDefaultAsync();
                
                if (existingTimeline != null)
                {
                    throw new ServiceException($"An active timeline of type {(TimelineTypeEnum)request.TimelineType} already exists. Please deactivate it before creating a new one.");
                }
                
                // Validate the chronological order between ProjectRegistration and ReviewPeriod
                if (request.TimelineType == (int)TimelineTypeEnum.ProjectRegistration)
                {
                    // If this is a ProjectRegistration timeline, check if any ReviewPeriod timeline exists
                    var reviewTimeline = await _context.Timelines
                        .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod &&
                               t.Status == (int)TimelineStatusEnum.Active)
                        .FirstOrDefaultAsync();
                    
                    if (reviewTimeline != null && reviewTimeline.StartDate.HasValue)
                    {
                        // Ensure the ProjectRegistration ends before ReviewPeriod starts
                        if (request.EndDate > reviewTimeline.StartDate)
                        {
                            throw new ServiceException($"Project registration must end before review period starts. Review period starts on {reviewTimeline.StartDate:yyyy-MM-dd}.");
                        }
                    }
                }
                else if (request.TimelineType == (int)TimelineTypeEnum.ReviewPeriod)
                {
                    // If this is a ReviewPeriod timeline, check if any ProjectRegistration timeline exists
                    var registrationTimeline = await _context.Timelines
                        .Where(t => t.TimelineType == (int)TimelineTypeEnum.ProjectRegistration &&
                               t.Status == (int)TimelineStatusEnum.Active)
                        .FirstOrDefaultAsync();
                    
                    if (registrationTimeline != null && registrationTimeline.EndDate.HasValue)
                    {
                        // Ensure the ReviewPeriod starts after ProjectRegistration ends
                        if (request.StartDate < registrationTimeline.EndDate)
                        {
                            throw new ServiceException($"Review period must start after project registration ends. Project registration ends on {registrationTimeline.EndDate:yyyy-MM-dd}.");
                        }
                    }
                }
            }
            
            // If no sequenceId is provided, use a default one or the most recent/active sequence
            if (request.SequenceId == null || request.SequenceId <= 0)
            {
                var defaultSequence = await _context.TimelineSequence
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();
                    
                if (defaultSequence == null)
                {
                    throw new ServiceException("No timeline sequence exists. Please create a sequence first.");
                }
                
                request.SequenceId = defaultSequence.SequenceId;
            }

            // Validate that the sequence exists and is active
            var sequence = await _context.TimelineSequence.FindAsync(request.SequenceId);
            if (sequence == null)
            {
                throw new ServiceException("Timeline sequence not found.");
            }

            if (sequence.Status != (int)TimelineSequenceStatusEnum.Active)
            {
                throw new ServiceException("Cannot add a timeline to an inactive or archived sequence.");
            }

            // Set default status to Active if not provided
            if (request.Status == null)
            {
                request.Status = (int)TimelineStatusEnum.Active;
            }

            var timeline = new Timeline
            {
                SequenceId = request.SequenceId,
                StartDate = request.StartDate.Value,
                EndDate = request.EndDate.Value,
                Event = request.Event,
                TimelineType = request.TimelineType,
                Status = request.Status,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            await _context.Timelines.AddAsync(timeline);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.FindAsync(createdBy);
            var sequenceDetails = await _context.Set<TimelineSequence>().FindAsync(request.SequenceId);
            
            string statusName = timeline.Status.HasValue 
                ? Enum.GetName(typeof(TimelineStatusEnum), timeline.Status) 
                : "Unknown";
                
            // Send notifications to all students and lecturers - non-blocking
            SendTimelineNotifications(timeline, sequenceDetails, createdUser);
            
            return new TimelineResponse
            {
                Id = timeline.TimelineId,
                SequenceId = timeline.SequenceId,
                SequenceName = sequenceDetails?.SequenceName,
                SequenceColor = sequenceDetails?.SequenceColor,
                StartDate = timeline.StartDate,
                EndDate = timeline.EndDate,
                Event = timeline.Event,
                CreatedAt = timeline.CreatedAt,
                UpdateAt = timeline.UpdateAt,
                TimelineType = timeline.TimelineType,
                Status = timeline.Status,
                StatusName = statusName,
                CreatedBy = timeline.CreatedBy,
                CreatedByName = createdUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    // Helper method to send notifications to lecturers and students
    private Task SendTimelineNotifications(Timeline timeline, TimelineSequence sequence, User creator)
    {
        // Fire and forget task - don't await this
        _ = Task.Run(async () =>
        {
            try
            {
                // Get timeline type name for the message
                string timelineTypeName = timeline.TimelineType.HasValue 
                    ? Enum.GetName(typeof(TimelineTypeEnum), timeline.TimelineType) 
                    : "Unknown";
                    
                // Format the notification title and message
                string title = "New Timeline Period Created";
                string message = $"A new timeline for {timelineTypeName} has been created from {timeline.StartDate:yyyy-MM-dd} to {timeline.EndDate:yyyy-MM-dd}.";
                
                if (!string.IsNullOrEmpty(timeline.Event))
                {
                    message += $" Event: {timeline.Event}";
                }
                
                if (sequence != null && !string.IsNullOrEmpty(sequence.SequenceName))
                {
                    message += $" Timeline Sequence: {sequence.SequenceName}";
                }
                
                // Create a scope to get a new DbContext instance to avoid disposed context issues
                using (var scope = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddDbContext<LRMSDbContext>(options => options.UseSqlServer(_context.Database.GetConnectionString()))
                    .AddScoped<INotificationService, NotificationService>()
                    .BuildServiceProvider())
                {
                    // Get all users with Student and Lecturer roles using a new context
                    using var newContext = scope.GetRequiredService<LRMSDbContext>();
                    var notificationService = scope.GetRequiredService<INotificationService>();
                    
                    var users = await newContext.Users
                        .Where(u => u.Role == (int)SystemRoleEnum.Student || u.Role == (int)SystemRoleEnum.Lecturer)
                        .ToListAsync();
                    
                    // Send notifications in batches to avoid overwhelming the system
                    int batchSize = 50;
                    for (int i = 0; i < users.Count; i += batchSize)
                    {
                        var userBatch = users.Skip(i).Take(batchSize);
                        var tasks = userBatch.Select(user => 
                        {
                            var notification = new CreateNotificationRequest
                            {
                                UserId = user.UserId,
                                Title = title,
                                Message = message,
                                Status = 0,
                                IsRead = false
                            };
                            
                            return notificationService.CreateNotification(notification);
                        });
                        
                        await Task.WhenAll(tasks);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent breaking the timeline creation
                Console.WriteLine($"Error sending timeline notifications: {ex.Message}");
            }
        });
        
        // Return completed task so we don't block
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<TimelineResponse>> GetAllTimelines()
    {
        try
        {
            var timelines = await _context.Timelines
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Sequence)
                .AsNoTracking()
                .ToListAsync();

            return timelines.Select(t => new TimelineResponse
            {
                Id = t.TimelineId,
                SequenceId = t.SequenceId,
                SequenceName = t.Sequence?.SequenceName,
                SequenceColor = t.Sequence?.SequenceColor,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Event = t.Event,
                CreatedAt = t.CreatedAt,
                UpdateAt = t.UpdateAt,
                TimelineType = t.TimelineType,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByNavigation?.FullName ?? "Unknown"
            });
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<TimelineResponse> GetTimelineById(int id)
    {
        try
        {
            var timeline = await _context.Timelines
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Sequence)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TimelineId == id);

            if (timeline == null)
                throw new ServiceException("Timeline not found");

            return new TimelineResponse
            {
                Id = timeline.TimelineId,
                SequenceId = timeline.SequenceId,
                SequenceName = timeline.Sequence?.SequenceName,
                SequenceColor = timeline.Sequence?.SequenceColor,
                StartDate = timeline.StartDate,
                EndDate = timeline.EndDate,
                Event = timeline.Event,
                CreatedAt = timeline.CreatedAt,
                UpdateAt = timeline.UpdateAt,
                TimelineType = timeline.TimelineType,
                CreatedBy = timeline.CreatedBy,
                CreatedByName = timeline.CreatedByNavigation?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }


    public async Task<IEnumerable<TimelineResponse>> GetTimelinesBySequenceId(int sequenceId)
    {
        try
        {
            var timelines = await _context.Timelines
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Sequence)
                .Where(t => t.SequenceId == sequenceId)
                .AsNoTracking()
                .ToListAsync();

            if (!timelines.Any())
                return Enumerable.Empty<TimelineResponse>();

            return timelines.Select(t => new TimelineResponse
            {
                Id = t.TimelineId,
                SequenceId = t.SequenceId,
                SequenceName = t.Sequence?.SequenceName,
                SequenceColor = t.Sequence?.SequenceColor,
                StartDate = t.StartDate?.Date,
                EndDate = t.EndDate?.Date,
                Event = t.Event,
                CreatedAt = t.CreatedAt,
                UpdateAt = t.UpdateAt,
                TimelineType = t.TimelineType,
                Status = t.Status,
                StatusName = t.Status.HasValue
                    ? Enum.GetName(typeof(TimelineStatusEnum), t.Status)
                    : null,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByNavigation?.FullName ?? "Unknown"
            });
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<TimelineResponse> UpdateTimeline(int id, TimelineRequest request, int updatedBy)
    {
        try
        {
            var timeline = await _context.Timelines.FindAsync(id);
            if (timeline == null)
            {
                throw new ServiceException("Timeline not found");
            }

            // Capture original values for change detection
            var originalStartDate = timeline.StartDate;
            var originalEndDate = timeline.EndDate;
            var originalEvent = timeline.Event;
            var originalStatus = timeline.Status;

            // Check chronological order when updating dates
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                DateTime newStartDate = request.StartDate ?? timeline.StartDate.Value;
                DateTime newEndDate = request.EndDate ?? timeline.EndDate.Value;
                
                // Validate the chronological order between ProjectRegistration and ReviewPeriod
                if (timeline.TimelineType == (int)TimelineTypeEnum.ProjectRegistration)
                {
                    // If this is a ProjectRegistration timeline, check if any ReviewPeriod timeline exists
                    var reviewTimeline = await _context.Timelines
                        .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod &&
                               t.Status == (int)TimelineStatusEnum.Active &&
                               t.TimelineId != id) // Exclude current timeline
                        .FirstOrDefaultAsync();
                    
                    if (reviewTimeline != null && reviewTimeline.StartDate.HasValue)
                    {
                        // Ensure the ProjectRegistration ends before ReviewPeriod starts
                        if (newEndDate > reviewTimeline.StartDate)
                        {
                            throw new ServiceException($"Project registration must end before review period starts");
                        }
                    }
                }
                else if (timeline.TimelineType == (int)TimelineTypeEnum.ReviewPeriod)
                {
                    // If this is a ReviewPeriod timeline, check if any ProjectRegistration timeline exists
                    var registrationTimeline = await _context.Timelines
                        .Where(t => t.TimelineType == (int)TimelineTypeEnum.ProjectRegistration &&
                               t.Status == (int)TimelineStatusEnum.Active &&
                               t.TimelineId != id) // Exclude current timeline
                        .FirstOrDefaultAsync();
                    
                    if (registrationTimeline != null && registrationTimeline.EndDate.HasValue)
                    {
                        // Ensure the ReviewPeriod starts after ProjectRegistration ends
                        if (newStartDate < registrationTimeline.EndDate)
                        {
                            throw new ServiceException($"Review period must start after project registration ends");
                        }
                    }
                }
            }

            // Update the timeline properties
            timeline.SequenceId = request.SequenceId ?? timeline.SequenceId;
            timeline.StartDate = request.StartDate ?? timeline.StartDate;
            timeline.EndDate = request.EndDate ?? timeline.EndDate;
            timeline.Event = request.Event ?? timeline.Event;
            timeline.TimelineType = request.TimelineType ?? timeline.TimelineType;
            timeline.Status = request.Status ?? timeline.Status;
            timeline.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users.FindAsync(updatedBy);
            var sequence = await _context.Set<TimelineSequence>().FindAsync(timeline.SequenceId);

            string statusName = timeline.Status.HasValue 
                ? Enum.GetName(typeof(TimelineStatusEnum), timeline.Status) 
                : "Unknown";
            
            // Check if important properties were changed
            bool datesChanged = (request.StartDate.HasValue && originalStartDate != timeline.StartDate) || 
                               (request.EndDate.HasValue && originalEndDate != timeline.EndDate);
            bool eventChanged = request.Event != null && originalEvent != timeline.Event;
            bool statusChanged = request.Status.HasValue && originalStatus != timeline.Status;
            
            // Send notifications if important details changed - non-blocking
            if (datesChanged || eventChanged || statusChanged)
            {
                SendTimelineUpdateNotifications(timeline, sequence, updatedUser, datesChanged, eventChanged, statusChanged);
            }
            
            return new TimelineResponse
            {
                Id = timeline.TimelineId,
                SequenceId = timeline.SequenceId,
                SequenceName = sequence?.SequenceName,
                SequenceColor = sequence?.SequenceColor,
                StartDate = timeline.StartDate,
                EndDate = timeline.EndDate,
                Event = timeline.Event,
                CreatedAt = timeline.CreatedAt,
                UpdateAt = timeline.UpdateAt,
                TimelineType = timeline.TimelineType,
                Status = timeline.Status,
                StatusName = statusName,
                CreatedBy = timeline.CreatedBy,
                CreatedByName = updatedUser?.FullName ?? "Unknown"
            };

        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
    
    // Helper method to send notifications for timeline updates
    private Task SendTimelineUpdateNotifications(Timeline timeline, TimelineSequence sequence, User updater, 
                                                 bool datesChanged, bool eventChanged, bool statusChanged)
    {
        // Fire and forget task - don't await this
        _ = Task.Run(async () =>
        {
            try
            {
                // Get timeline type name for the message
                string timelineTypeName = timeline.TimelineType.HasValue 
                    ? Enum.GetName(typeof(TimelineTypeEnum), timeline.TimelineType) 
                    : "Unknown";
                    
                // Format the notification title and message
                string title = "Timeline Updated";
                
                // Build a detailed message based on what changed
                string message = $"The timeline for {timelineTypeName} has been updated.";
                
                if (datesChanged)
                {
                    message += $" The date range has been changed to {timeline.StartDate:yyyy-MM-dd} - {timeline.EndDate:yyyy-MM-dd}.";
                }
                
                if (eventChanged && !string.IsNullOrEmpty(timeline.Event))
                {
                    message += $" Event description updated to: {timeline.Event}";
                }
                
                if (statusChanged && timeline.Status.HasValue)
                {
                    string statusName = Enum.GetName(typeof(TimelineStatusEnum), timeline.Status);
                    message += $" Status changed to: {statusName}";
                }
                
                if (sequence != null && !string.IsNullOrEmpty(sequence.SequenceName))
                {
                    message += $" Timeline Sequence: {sequence.SequenceName}";
                }
                
                // Create a scope to get a new DbContext instance to avoid disposed context issues
                using (var scope = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddDbContext<LRMSDbContext>(options => options.UseSqlServer(_context.Database.GetConnectionString()))
                    .AddScoped<INotificationService, NotificationService>()
                    .BuildServiceProvider())
                {
                    // Get all users with Student and Lecturer roles using a new context
                    using var newContext = scope.GetRequiredService<LRMSDbContext>();
                    var notificationService = scope.GetRequiredService<INotificationService>();
                    
                    var users = await newContext.Users
                        .Where(u => u.Role == (int)SystemRoleEnum.Student || u.Role == (int)SystemRoleEnum.Lecturer)
                        .ToListAsync();
                    
                    // Send notifications in batches to avoid overwhelming the system
                    int batchSize = 50;
                    for (int i = 0; i < users.Count; i += batchSize)
                    {
                        var userBatch = users.Skip(i).Take(batchSize);
                        var tasks = userBatch.Select(user => 
                        {
                            var notification = new CreateNotificationRequest
                            {
                                UserId = user.UserId,
                                Title = title,
                                Message = message,
                                Status = 0,
                                IsRead = false
                            };
                            
                            return notificationService.CreateNotification(notification);
                        });
                        
                        await Task.WhenAll(tasks);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent breaking the timeline update
                Console.WriteLine($"Error sending timeline update notifications: {ex.Message}");
            }
        });
        
        // Return completed task so we don't block
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteTimeline(int id)
    {
        try
        {
            var timeline = await _context.Timelines.FindAsync(id);
            if (timeline == null)
            {
                throw new ServiceException("Timeline not found");
            }

            // Check if the timeline is being used by any project requests
            var isBeingUsed = await _context.ProjectRequests
                .AnyAsync(pr => pr.TimelineId == id);
                
            if (isBeingUsed)
            {
                throw new ServiceException("Cannot delete this timeline as it is being used by one or more project requests");
            }

            _context.Timelines.Remove(timeline);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 