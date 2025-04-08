using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;

namespace Service.Interfaces;

public interface IProjectService
{
    Task<int> CreateResearchProject(CreateProjectRequest request, IFormFile documentFile, int createdBy);
    Task<bool> SendProjectForApproval(ProjectApprovalRequest request);
    Task<bool> ApproveProject(int projectId, int approvedBy);
    Task<bool> RejectProject(int projectId, int rejectedBy, string reason);

    Task<IEnumerable<ProjectResponse>> GetAllProjects(); 
    Task<ProjectResponse> GetProjectById(int projectId);
    Task<IEnumerable<ProjectResponse>> GetProjectsByDepartmentId(int departmentId);
    Task<IEnumerable<ProjectResponse>> GetProjectsByUserId(int userId);
}
