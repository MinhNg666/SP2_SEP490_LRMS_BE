using Domain.Constants;
using LRMS_API;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ITimelineValidationService
    {
        Task<bool> IsValidTimeForAction(TimelineTypeEnum timelineType, int? sequenceId = null, bool strictValidation = false);
        Task<Timeline> GetActiveTimeline(TimelineTypeEnum timelineType, int? sequenceId = null);
    }
} 