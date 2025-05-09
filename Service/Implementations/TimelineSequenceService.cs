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

public class TimelineSequenceService : ITimelineSequenceService
{
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public TimelineSequenceService(IMapper mapper, LRMSDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<TimelineSequenceResponse> CreateTimelineSequence(TimelineSequenceRequest request, int createdBy)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SequenceName))
            {
                throw new ServiceException("Sequence Name is required");
            }

            // Set default status to Active if not provided
            if (request.Status == null)
            {
                request.Status = (int)TimelineSequenceStatusEnum.Active;
            }

            var sequence = new TimelineSequence
            {
                SequenceName = request.SequenceName,
                SequenceDescription = request.SequenceDescription,
                SequenceColor = request.SequenceColor,
                Status = request.Status,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.TimelineSequence.AddAsync(sequence);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.FindAsync(createdBy);
            
            string statusName = sequence.Status.HasValue 
                ? Enum.GetName(typeof(TimelineSequenceStatusEnum), sequence.Status) 
                : "Unknown";
            
            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                Status = sequence.Status,
                StatusName = statusName,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = createdUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<IEnumerable<TimelineSequenceResponse>> GetAllTimelineSequences()
    {
        try
        {
            var sequences = await _context.TimelineSequence
                .Include(t => t.CreatedByNavigation)
                .AsNoTracking()
                .ToListAsync();

            return sequences.Select(s => new TimelineSequenceResponse
            {
                Id = s.SequenceId,
                SequenceName = s.SequenceName,
                SequenceDescription = s.SequenceDescription,
                SequenceColor = s.SequenceColor,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                CreatedByName = s.CreatedByNavigation?.FullName ?? "Unknown"
            });
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<TimelineSequenceResponse> GetTimelineSequenceById(int id)
    {
        try
        {
            var sequence = await _context.TimelineSequence
                .Include(t => t.CreatedByNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.SequenceId == id);

            if (sequence == null)
                throw new ServiceException("Timeline Sequence not found");

            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = sequence.CreatedByNavigation?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<TimelineSequenceResponse> UpdateTimelineSequence(int id, TimelineSequenceRequest request, int updatedBy)
    {
        try
        {
            var sequence = await _context.TimelineSequence.FindAsync(id);
            if (sequence == null)
            {
                throw new ServiceException("Timeline Sequence not found");
            }

            // Store old status to check if status has changed
            var oldStatus = sequence.Status;

            // Update the sequence properties
            sequence.SequenceName = request.SequenceName ?? sequence.SequenceName;
            sequence.SequenceDescription = request.SequenceDescription ?? sequence.SequenceDescription;
            sequence.SequenceColor = request.SequenceColor ?? sequence.SequenceColor;
            sequence.Status = request.Status ?? sequence.Status;
            sequence.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // If status has changed, update associated timelines accordingly
            if (oldStatus != sequence.Status && request.Status.HasValue)
            {
                // Map sequence status to timeline status based on business rules
                int? timelineStatus = null;
                
                // If sequence is inactive, mark timelines as inactive
                if (sequence.Status == (int)TimelineSequenceStatusEnum.Inactive)
                {
                    timelineStatus = (int)TimelineStatusEnum.Inactive;
                }
                // If sequence is archived, mark timelines as completed
                else if (sequence.Status == (int)TimelineSequenceStatusEnum.Archived)
                {
                    timelineStatus = (int)TimelineStatusEnum.Completed;
                }
                // If sequence is reactivated, only reactivate timelines that are not completed or cancelled
                else if (sequence.Status == (int)TimelineSequenceStatusEnum.Active)
                {
                    // This is a special case handled in the UpdateTimelineStatusesInSequence method
                    await UpdateTimelineStatusesInSequence(id, (int)TimelineStatusEnum.Active, updatedBy);
                }
                
                // Update timeline statuses if needed
                if (timelineStatus.HasValue)
                {
                    await UpdateTimelineStatusesInSequence(id, timelineStatus.Value, updatedBy);
                }
            }

            var updatedUser = await _context.Users.FindAsync(updatedBy);
            
            string statusName = sequence.Status.HasValue 
                ? Enum.GetName(typeof(TimelineSequenceStatusEnum), sequence.Status) 
                : "Unknown";

            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                Status = sequence.Status,
                StatusName = statusName,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = updatedUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<bool> UpdateTimelineStatusesInSequence(int sequenceId, int newStatus, int updatedBy)
    {
        try
        {
            var sequence = await _context.TimelineSequence.FindAsync(sequenceId);
            if (sequence == null)
            {
                throw new ServiceException("Timeline Sequence not found");
            }

            var timelines = await _context.Timelines
                .Where(t => t.SequenceId == sequenceId)
                .ToListAsync();

            if (!timelines.Any())
            {
                return true; // No timelines to update
            }

            foreach (var timeline in timelines)
            {
                // Special handling for reactivation
                if (newStatus == (int)TimelineStatusEnum.Active && 
                    (timeline.Status == (int)TimelineStatusEnum.Completed || 
                     timeline.Status == (int)TimelineStatusEnum.Cancelled))
                {
                    // Skip completed or cancelled timelines when reactivating
                    continue;
                }

                timeline.Status = newStatus;
                timeline.UpdateAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<bool> DeleteTimelineSequence(int id)
    {
        try
        {
            var sequence = await _context.TimelineSequence.FindAsync(id);
            if (sequence == null)
            {
                throw new ServiceException("Timeline Sequence not found");
            }

            // Check if the sequence is being used by any timelines
            var isUsedByTimelines = await _context.Timelines
                .AnyAsync(t => t.SequenceId == id);
                
            if (isUsedByTimelines)
            {
                throw new ServiceException("Cannot delete this sequence as it is being used by one or more timelines");
            }
            
            // Check if the sequence is being used by any projects
            var isUsedByProjects = await _context.Projects
                .AnyAsync(p => p.SequenceId == id);
                
            if (isUsedByProjects)
            {
                throw new ServiceException("Cannot delete this sequence as it is being used by one or more projects");
            }

            _context.TimelineSequence.Remove(sequence);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 