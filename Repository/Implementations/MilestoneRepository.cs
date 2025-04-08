using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;
public class MilestoneRepository : GenericRepository<Milestone>, IMilestoneRepository
{
    private readonly LRMSDbContext _context;
    public MilestoneRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<int> AddMilestoneAsync(Milestone milestone)
       {
           await _context.Milestones.AddAsync(milestone);
           await _context.SaveChangesAsync();
           return milestone.MilestoneId;
       }
}
