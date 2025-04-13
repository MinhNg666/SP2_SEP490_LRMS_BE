using LRMS_API;

namespace Repository.Interfaces;

public interface IJournalRepository : IGenericRepository<Journal>
{
    Task<IEnumerable<Journal>> GetAllJournalsWithDetailsAsync();
    Task<Journal> GetJournalWithDetailsAsync(int journalId);
} 