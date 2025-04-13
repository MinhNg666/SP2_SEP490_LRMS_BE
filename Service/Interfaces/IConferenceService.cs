using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IConferenceService
{
    Task<int> CreateConference(CreateConferenceRequest request, int createdBy);
    Task AddConferenceDocument(int conferenceId, IFormFile documentFile, int userId);
    Task<IEnumerable<ConferenceResponse>> GetAllConferences();
    Task<ConferenceResponse> GetConferenceById(int conferenceId);
    Task<IEnumerable<ConferenceResponse>> GetConferencesByProjectId(int projectId);
} 