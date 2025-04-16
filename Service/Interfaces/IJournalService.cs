using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;
public interface IJournalService
{
    Task<int> CreateJournalFromResearch(int projectId, int userId, CreateJournalFromProjectRequest request);
    Task<bool> AddJournalDocument(int journalId, int userId, IFormFile documentFile);
    Task<bool> ApproveJournal(int journalId, int secretaryId, IFormFile documentFile);
    Task<bool> RejectJournal(int journalId, int secretaryId, IFormFile documentFile);
}
