using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;
public interface IJournalService
{
    Task<JournalResponse> CreateJournal(CreateJournalRequest request, int createdBy);
    Task<int> CreateJournalFromResearch(int projectId, int userId, CreateJournalFromProjectRequest request);
    Task<IEnumerable<JournalResponse>> GetAllJournals();
    Task<JournalResponse> GetJournalById(int journalId);
    Task<IEnumerable<JournalResponse>> GetJournalsByProjectId(int projectId);
    Task AddJournalDocuments(int journalId, IEnumerable<IFormFile> documentFiles, int userId);
    Task<bool> AddJournalDocument(int journalId, int userId, IFormFile documentFile);
    Task<bool> ApproveJournal(int journalId, int secretaryId, IFormFile documentFile);
    Task<bool> RejectJournal(int journalId, int secretaryId, IFormFile documentFile);
} 