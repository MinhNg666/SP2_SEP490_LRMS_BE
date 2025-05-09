using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class TimelineManagementService : ITimelineManagementService
    {
        private readonly LRMSDbContext _context;
        private readonly ITimelineService _timelineService;

        public TimelineManagementService(LRMSDbContext context, ITimelineService timelineService)
        {
            _context = context;
            _timelineService = timelineService;
        }

        public async Task<IEnumerable<TimelineResponse>> GetCurrentActiveTimelines()
        {
            var currentDate = DateTime.Now;
            var timelines = await _context.Timelines
                .Include(t => t.Sequence)
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.StartDate <= currentDate && t.EndDate >= currentDate && 
                       t.Status == (int)TimelineStatusEnum.Active)
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
                Status = t.Status,
                StatusName = t.Status.HasValue 
                    ? Enum.GetName(typeof(TimelineStatusEnum), t.Status) 
                    : "Unknown",
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByNavigation?.FullName ?? "Unknown"
            }).ToList();
        }

        public async Task<TimelineResponse> CreateActionTimeline(TimelineRequest request, int createdBy)
        {
            // Validate no overlapping timelines of same type
            var overlappingTimeline = await _context.Timelines
                .Where(t => t.TimelineType == request.TimelineType &&
                      t.Status == (int)TimelineStatusEnum.Active &&
                      ((t.StartDate <= request.StartDate && t.EndDate >= request.StartDate) ||
                       (t.StartDate <= request.EndDate && t.EndDate >= request.EndDate) ||
                       (t.StartDate >= request.StartDate && t.EndDate <= request.EndDate)))
                .FirstOrDefaultAsync();

            if (overlappingTimeline != null)
                throw new ServiceException($"An active timeline of type {(TimelineTypeEnum)request.TimelineType} already exists during this period");

            // Create timeline
            return await _timelineService.CreateTimeline(request, createdBy);
        }

        public async Task<bool> ExtendTimeline(int timelineId, DateTime newEndDate, int updatedBy)
        {
            var timeline = await _context.Timelines.FindAsync(timelineId);
            if (timeline == null)
                throw new ServiceException("Timeline not found");

            if (newEndDate <= timeline.EndDate)
                throw new ServiceException("New end date must be after current end date");

            timeline.EndDate = newEndDate;
            timeline.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
} 