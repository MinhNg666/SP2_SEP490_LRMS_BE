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
            .Include(f => f.Project)
            .Include(f => f.AuthorRequestNavigation)
                .ThenInclude(a => a.User)
            .Include(f => f.SupervisorRequestNavigation)
                .ThenInclude(s => s.User)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .ToListAsync();
    }
    
    public async Task<FundDisbursement> GetByIdWithDetailsAsync(int fundDisbursementId)
    {
        return await _context.FundDisbursements
            .Include(f => f.Project)
            .Include(f => f.AuthorRequestNavigation)
                .ThenInclude(a => a.User)
            .Include(f => f.SupervisorRequestNavigation)
                .ThenInclude(s => s.User)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.FundDisbursementId == fundDisbursementId);
    }
    
    public async Task<IEnumerable<FundDisbursement>> GetByProjectIdAsync(int projectId)
    {
        return await _context.FundDisbursements
            .Include(f => f.Project)
            .Include(f => f.AuthorRequestNavigation)
                .ThenInclude(a => a.User)
            .Include(f => f.SupervisorRequestNavigation)
                .ThenInclude(s => s.User)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .Where(f => f.ProjectId == projectId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<FundDisbursement>> GetByUserIdAsync(int userId)
    {
        return await _context.FundDisbursements
            .Include(f => f.Project)
            .Include(f => f.AuthorRequestNavigation)
                .ThenInclude(a => a.User)
            .Include(f => f.SupervisorRequestNavigation)
                .ThenInclude(s => s.User)
            .Include(f => f.ProjectPhase)
            .Include(f => f.Quota)
            .Include(f => f.Documents)
            .Where(f => f.AuthorRequestNavigation.UserId == userId || 
                        f.SupervisorRequestNavigation.UserId == userId)
            .ToListAsync();
    }
}
