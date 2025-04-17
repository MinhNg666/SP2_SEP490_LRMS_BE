using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;

public class ConferenceRepository : GenericRepository<Conference>, IConferenceRepository
{
    private readonly LRMSDbContext _context;

    public ConferenceRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<int> AddExpenseAsync(ConferenceExpense expense)
    {
        await _context.ConferenceExpenses.AddAsync(expense);
        await _context.SaveChangesAsync();
        return expense.ExpenseId;
    }

    public async Task<IEnumerable<Conference>> GetAllConferencesWithDetailsAsync()
    {
        return await _context.Conferences
            .Include(c => c.Project)
                .ThenInclude(p => p.Department)
            .Include(c => c.ConferenceExpenses)
                .ThenInclude(e => e.Documents)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Conference?> GetConferenceWithDetailsAsync(int conferenceId)
    {
        return await _context.Conferences
            .Include(c => c.Project)
                .ThenInclude(p => p.Department)
            .Include(c => c.ConferenceExpenses)
                .ThenInclude(e => e.Documents)
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
    }
} 