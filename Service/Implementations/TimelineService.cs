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

namespace Service.Implementations;

public class TimelineService : ITimelineService
{
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public TimelineService(IMapper mapper, LRMSDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<TimelineResponse> CreateTimeline(TimelineRequest request, int createdBy)
    {
        try
        {
            if (request.StartDate == null || request.EndDate == null)
            {
                throw new ServiceException("StartDate and EndDate are required");
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
            var sequence = await _context.Set<TimelineSequence>().FindAsync(request.SequenceId);
            
            string statusName = timeline.Status.HasValue 
                ? Enum.GetName(typeof(TimelineStatusEnum), timeline.Status) 
                : "Unknown";
            
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
                CreatedByName = createdUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
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