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

    public async Task<TimelineResponse> CreateTimeline(TimelineRequest request)
    {
        try
        {
            if (request.StartDate == null || request.EndDate == null || request.CreatedBy == null)
            {
                throw new ServiceException("StartDate, EndDate and CreatedBy are required");
            }

            var timeline = new Timeline
            {
                SequenceId = request.SequenceId,
                StartDate = request.StartDate.Value,
                EndDate = request.EndDate.Value,
                Event = request.Event,
                TimelineType = request.TimelineType,
                CreatedBy = request.CreatedBy.Value,
                CreatedAt = DateTime.Now
            };

            await _context.Timelines.AddAsync(timeline);
            await _context.SaveChangesAsync();

            var createdUser = request.CreatedBy != null ? await _context.Users.FindAsync(request.CreatedBy) : null;
            var sequence = request.SequenceId != null ? await _context.Set<TimelineSequence>().FindAsync(request.SequenceId) : null;
            
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

    public async Task<bool> IsValidTimeForAction(int timelineType, int? sequenceId = null)
    {
        try
        {
            var now = DateTime.Now;

            var query = _context.Timelines
                .Where(t => t.TimelineType == timelineType);

            if (sequenceId.HasValue)
            {
                query = query.Where(t => t.SequenceId == sequenceId);
            }

            var timeline = await query
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (timeline == null)
                throw new ServiceException("Timeline not found for this action");

            return now >= timeline.StartDate && now <= timeline.EndDate;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 