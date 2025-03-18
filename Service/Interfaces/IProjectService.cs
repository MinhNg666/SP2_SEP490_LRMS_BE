using Domain.DTO.Requests;

namespace Service.Interfaces;

public interface IProjectService
{
    Task<int> CreateResearchProject(CreateProjectRequest request, int createdBy);
    Task<bool> SendProjectForApproval(ProjectApprovalRequest request);
    Task<bool> ApproveProject(int projectId, int approvedBy);
    Task<bool> RejectProject(int projectId, int rejectedBy, string reason);
}
