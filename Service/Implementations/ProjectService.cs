using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Service.Implementations;
public class ProjectService : IProjectService
{
    private readonly IS3Service _s3Service;
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,
        INotificationService notificationService, IMapper mapper)
    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<int> CreateResearchProject(CreateProjectRequest request, IFormFile documentFile, int createdBy)
    {
        try
        {
            var project = new Project
            {
            ProjectName = request.ProjectName,
            Description = request.Description,
            Methodlogy = request.Methodology,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApprovedBudget = request.ApprovedBudget,
            Status = (int)ProjectStatusEnum.Pending,
            CreatedAt = DateTime.Now,
            CreatedBy = createdBy,
            GroupId = request.GroupId,
            DepartmentId = request.DepartmentId,
            ProjectType = (int)ProjectTypeEnum.Research
        };

        await _projectRepository.AddAsync(project);
        if (documentFile != null)
            {
                var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{project.ProjectId}/documents");

                // 3. Tạo document record
                var document = new Document
                {
                    ProjectId = project.ProjectId,
                    DocumentUrl = documentUrl,
                    FileName = documentFile.FileName,
                    DocumentType = (int)DocumentTypeEnum.ProjectProposal,
                    UploadAt = DateTime.Now,
                    UploadedBy = createdBy
                };

                await _projectRepository.AddDocumentAsync(document);
            }
        return project.ProjectId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating research project: {ex.Message}");
        }
    }

    public async Task<bool> SendProjectForApproval(ProjectApprovalRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project == null)
            throw new ServiceException("Project not found");

        var councilGroup = await _groupRepository.GetByIdAsync(request.CouncilGroupId);
        if (councilGroup == null)
            throw new ServiceException("Council group not found");

        // Gửi thông báo cho tất cả thành viên trong council group
        var councilMembers = await _groupRepository.GetMembersByGroupId(request.CouncilGroupId);

        foreach (var member in councilMembers)
        {
            var notificationRequest = new CreateNotificationRequest
            {
                UserId = member.UserId.Value,
                Title = "New Project Approval Request",
                Message = $"A new research project '{project.ProjectName}' needs your approval",
                ProjectId = project.ProjectId
            };

            await _notificationService.CreateNotification(notificationRequest);
        }

        return true;
    }

    public async Task<bool> ApproveProject(int projectId, int approvedBy)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new ServiceException("Project not found");

        project.Status = (int)ProjectStatusEnum.Approved;
        project.ApprovedBy = approvedBy;
        project.UpdatedAt = DateTime.Now;

        await _projectRepository.UpdateAsync(project);

        // Thông báo cho nhóm nghiên cứu
        var researchGroup = await _groupRepository.GetByIdAsync(project.GroupId.Value);
        var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);

        foreach (var member in groupMembers)
        {
            var notificationRequest = new CreateNotificationRequest
            {
                UserId = member.UserId.Value,
                Title = "Project Approved",
                Message = $"Your research project '{project.ProjectName}' has been approved",
                ProjectId = project.ProjectId
            };

            await _notificationService.CreateNotification(notificationRequest);
        }

        return true;
    }

    public async Task<bool> RejectProject(int projectId, int rejectedBy, string reason)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new ServiceException("Project not found");

        project.Status = (int)ProjectStatusEnum.Rejected;
        project.UpdatedAt = DateTime.Now;

        await _projectRepository.UpdateAsync(project);

        // Thông báo cho nhóm nghiên cứu
        var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
        foreach (var member in groupMembers)
        {
            var notificationRequest = new CreateNotificationRequest
            {
                UserId = member.UserId.Value,
                Title = "Project Rejected",
                Message = $"Your research project '{project.ProjectName}' has been rejected. Reason: {reason}",
                ProjectId = project.ProjectId
            };

            await _notificationService.CreateNotification(notificationRequest);
        }

        return true;
    }
}
