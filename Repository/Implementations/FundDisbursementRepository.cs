using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Implementations;
public class FundDisbursementRepository : GenericRepository<FundDisbursement>, IFundDisbursementRepository
{
    private readonly LRMSDbContext _context;
    
    public FundDisbursementRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<FundDisbursement>> GetAllWithDetailsAsync()
    {
        return await _context.FundDisbursements
            .AsNoTracking() // Keep this
            .Include(f => f.Project)
            .Include(f => f.UserRequestNavigation)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .Include(f => f.AppovedByNavigation)
                .ThenInclude(gm => gm.User)
            .Include(f => f.DisburseByNavigation)
            .ToListAsync();
    }
    
    public async Task<FundDisbursement> GetByIdWithDetailsAsync(int fundDisbursementId)
    {
        return await _context.FundDisbursements
            .Include(fd => fd.Project)
                .ThenInclude(p => p.ProjectPhases)
            .Include(fd => fd.Project)
                .ThenInclude(p => p.Department)
            .Include(fd => fd.Project)
                .ThenInclude(p => p.Group)
            .Include(fd => fd.Project)
                .ThenInclude(p => p.FundDisbursements)
            .Include(fd => fd.UserRequestNavigation)
            .Include(fd => fd.Quota)
                .ThenInclude(q => q.AllocatedByNavigation)
            .Include(fd => fd.ProjectPhase)
            .Include(fd => fd.Documents)
            .Include(fd => fd.AppovedByNavigation)
                .ThenInclude(gm => gm.User)
            .Include(fd => fd.DisburseByNavigation)
            .FirstOrDefaultAsync(fd => fd.FundDisbursementId == fundDisbursementId);
    }
    
    public async Task<IEnumerable<FundDisbursement>> GetByProjectIdAsync(int projectId)
    {
        return await _context.FundDisbursements
            .Include(f => f.Project)
            .Include(f => f.UserRequestNavigation)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .Include(f => f.AppovedByNavigation)
                .ThenInclude(gm => gm.User)
            .Include(f => f.DisburseByNavigation)
            .Where(f => f.ProjectId == projectId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<FundDisbursement>> GetByUserIdAsync(int userId)
    {
        return await _context.FundDisbursements
            .Include(f => f.Project)
            .Include(f => f.UserRequestNavigation)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .Include(f => f.AppovedByNavigation)
                .ThenInclude(gm => gm.User)
            .Include(f => f.DisburseByNavigation)
            .Where(f => f.UserRequest == userId)
            .ToListAsync();
    }
}
