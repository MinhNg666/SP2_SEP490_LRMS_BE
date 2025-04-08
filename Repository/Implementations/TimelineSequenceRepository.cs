using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;

public class TimelineSequenceRepository : GenericRepository<TimelineSequence>, ITimelineSequenceRepository
{
    public TimelineSequenceRepository(LRMSDbContext context) : base(context)
    {
    }
} 