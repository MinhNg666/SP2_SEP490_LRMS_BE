using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IProjectService
{
    Task<int> CreateResearchProject(CreateProjectRequest request, IFormFile documentFile, int createdBy);
    Task<bool> SendProjectForApproval(ProjectApprovalRequest request);
    Task<IEnumerable<ProjectResponse>> GetAllProjects(); 
    Task<ProjectResponse> GetProjectById(int projectId);
    Task<IEnumerable<ProjectResponse>> GetProjectsByDepartmentId(int departmentId);
    Task<IEnumerable<ProjectResponse>> GetProjectsByUserId(int userId);
    Task<bool> ApproveProjectBySecretary(int projectId, int secretaryId, IEnumerable<IFormFile> documentFiles);
    Task<bool> RejectProjectBySecretary(int projectId, int secretaryId, IEnumerable<IFormFile> documentFiles);
    Task AddProjectDocument(int projectId, IFormFile documentFile, int userId);
    Task<ProjectDetailResponse> GetProjectDetails(int projectId);
    Task<IEnumerable<ProjectListResponse>> GetUserPendingProjectsList(int userId);
    Task<IEnumerable<ProjectListResponse>> GetUserApprovedProjectsList(int userId);
    Task<bool> UpdateProjectPhaseStatus(int projectPhaseId, int status, int userId);
    Task UpdateProjectPhaseStatusesBasedOnDates();
    Task<bool> UpdateProjectPhase(int projectPhaseId, int status, decimal spentBudget, DateTime? startDate, DateTime? endDate, string title, int userId);
    Task AddProjectDocuments(int projectId, IEnumerable<IFormFile> documentFiles, int userId);
}
