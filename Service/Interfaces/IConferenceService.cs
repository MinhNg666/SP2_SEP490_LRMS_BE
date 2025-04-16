using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;
public interface IConferenceService
{
    Task<int> CreateConferenceFromResearch(int projectId, int leaderId, CreateConferenceFromProjectRequest request, IFormFile documentFile);
    Task<bool> AddConferenceDocument(int conferenceId, int userId, IFormFile documentFile);
    Task<bool> ApproveConference(int conferenceId, int secretaryId, IFormFile documentFile);
    Task<bool> RejectConference(int conferenceId, int secretaryId, IFormFile documentFile);
}
