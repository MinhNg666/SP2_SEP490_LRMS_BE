using Domain.Constants;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class TimelineValidationService : ITimelineValidationService
    {
        private readonly LRMSDbContext _context;

        public TimelineValidationService(LRMSDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsValidTimeForAction(TimelineTypeEnum timelineType, int? sequenceId = null, bool strictValidation = false)
        {
            var activeTimeline = await GetActiveTimeline(timelineType, sequenceId);
            
            if (activeTimeline == null && strictValidation)
            {
                throw new ServiceException($"No active timeline found for {timelineType}. This action can only be performed during the appropriate timeline.");
            }
            
            return activeTimeline != null;
        }

        public async Task<Timeline> GetActiveTimeline(TimelineTypeEnum timelineType, int? sequenceId = null)
        {
            var currentDate = DateTime.Now;
            var query = _context.Timelines
                .Include(t => t.Sequence)
                .Where(t => t.TimelineType == (int)timelineType &&
                       t.StartDate <= currentDate &&
                       t.EndDate >= currentDate &&
                       t.Status == (int)TimelineStatusEnum.Active &&
                       t.Sequence.Status == (int)TimelineSequenceStatusEnum.Active);

            if (sequenceId.HasValue)
            {
                query = query.Where(t => t.SequenceId == sequenceId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
} 