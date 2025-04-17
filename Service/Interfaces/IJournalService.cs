using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IJournalService
{
    Task<JournalResponse> CreateJournal(CreateJournalRequest request, int createdBy);
    Task AddJournalDocument(int journalId, IFormFile documentFile, int userId);
    Task<IEnumerable<JournalResponse>> GetAllJournals();
    Task<JournalResponse> GetJournalById(int journalId);
    Task<IEnumerable<JournalResponse>> GetJournalsByProjectId(int projectId);
    Task AddJournalDocuments(int journalId, IEnumerable<IFormFile> documentFiles, int userId);
} 