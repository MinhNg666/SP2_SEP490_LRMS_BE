using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IConferenceService
{
    Task<ConferenceResponse> CreateConference(CreateConferenceRequest request, int createdBy);
    Task<int> CreateConferenceFromResearch(int projectId, int leaderId, CreateConferenceFromProjectRequest request);
    Task<IEnumerable<ConferenceResponse>> GetAllConferences();
    Task<ConferenceResponse> GetConferenceById(int conferenceId);
    Task<IEnumerable<ConferenceResponse>> GetConferencesByProjectId(int projectId);
    Task AddConferenceDocuments(int conferenceId, IEnumerable<IFormFile> documentFiles, int userId);
    Task<bool> AddConferenceDocument(int conferenceId, int userId, IFormFile documentFile);
    Task<bool> ApproveConference(int conferenceId, int secretaryId, IFormFile documentFile);
    Task<bool> RejectConference(int conferenceId, int secretaryId, IFormFile documentFile);
    Task<bool> UpdateConferenceSubmission(int conferenceId, int userId, UpdateConferenceSubmissionRequest request);
    Task<bool> UpdateConferenceStatus(int conferenceId, int userId, int conferenceStatus);
    Task<bool> UpdateSubmissionStatus(int conferenceId, int userId, int submissionStatus, string reviewerComment);
    Task<bool> UpdateApprovedConferenceDetails(int conferenceId, int userId, UpdateApprovedConferenceRequest request);
    Task<int> CreateNewSubmissionAfterRejection(int projectId, int userId, int rejectedConferenceId, CreateNewSubmissionRequest request);
    Task<IEnumerable<ConferenceResponse>> GetUserConferences(int userId);
    Task<ConferenceDetailResponse> GetConferenceDetails(int conferenceId);
    Task<int> RequestConferenceExpenseAsync(int userId, RequestConferenceExpenseRequest request);
} 