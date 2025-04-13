using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Domain.Constants;

namespace Repository.Implementations;

public class JournalRepository : GenericRepository<Journal>, IJournalRepository
{
    private readonly LRMSDbContext _context;

    public JournalRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Journal>> GetAllJournalsWithDetailsAsync()
    {
        var journals = await _context.Journals
            .Include(j => j.Project)
                .ThenInclude(p => p.Documents.Where(d => d.DocumentType == (int)DocumentTypeEnum.JournalPaper))
            .AsNoTracking()
            .ToListAsync();

        return journals ?? new List<Journal>();
    }

    public async Task<Journal> GetJournalWithDetailsAsync(int journalId)
    {
        var journal = await _context.Journals
            .Include(j => j.Project)
                .ThenInclude(p => p.Documents.Where(d => d.DocumentType == (int)DocumentTypeEnum.JournalPaper))
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JournalId == journalId);

        return journal;
    }
} 