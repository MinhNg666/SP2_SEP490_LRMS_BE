using LRMS_API;
using Repository.Implementations;

namespace Repository.Interfaces;

public interface IConferenceRepository : IGenericRepository<Conference>
{
    Task<int> AddExpenseAsync(ConferenceExpense expense);
    Task<IEnumerable<Conference>> GetAllConferencesWithDetailsAsync();
    Task<Conference> GetConferenceWithDetailsAsync(int conferenceId);
} 