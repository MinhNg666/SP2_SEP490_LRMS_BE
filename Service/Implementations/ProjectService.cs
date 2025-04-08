using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Domain.DTO.Responses;

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
    public async Task<IEnumerable<ProjectResponse>> GetAllProjects()
    {
        try
        {
            var projects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            return _mapper.Map<IEnumerable<ProjectResponse>>(projects);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting all projects: {ex.Message}");
        }
    }

    public async Task<ProjectResponse> GetProjectById(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");
            
            return _mapper.Map<ProjectResponse>(project);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting project by id: {ex.Message}");
        }
    }
    public async Task<IEnumerable<ProjectResponse>> GetProjectsByDepartmentId(int departmentId)
    {
        try
        {
            var projects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            var departmentProjects = projects.Where(p => p.DepartmentId == departmentId);
            return _mapper.Map<IEnumerable<ProjectResponse>>(departmentProjects);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting project by department id: {ex.Message}");
        }
    }
    public async Task<IEnumerable<ProjectResponse>> GetProjectsByUserId(int userId)
    {
        try
        {
            // Lấy danh sách group mà user là thành viên
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            if (!userGroups.Any())
            {
                return Enumerable.Empty<ProjectResponse>();
            }

            // Lấy tất cả project có group ID nằm trong danh sách group của user
            var groupIds = userGroups.Select(g => g.GroupId);
            var projects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            var userProjects = projects.Where(p => p.GroupId.HasValue && groupIds.Contains(p.GroupId.Value));

            return _mapper.Map<IEnumerable<ProjectResponse>>(userProjects);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting project by user id: {ex.Message}");
        }
    }
    public async Task<int> CreateResearchProject(CreateProjectRequest request, IFormFile documentFile, int createdBy)
    {
        try
        {
            var existingProjects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            if (existingProjects.Any(p => p.ProjectName.Equals(request.ProjectName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ServiceException($"'{request.ProjectName}' has already exist. Please choose a different name.");
            }
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
                // 3. Tạo ProjectResource trước
                // var projectResource = new ProjectResource
                // {
                //     ResourceName = documentFile.FileName,
                //     ResourceType = 1, // Đặt loại resource là Document
                //     ProjectId = project.ProjectId,
                //     Acquired = true,
                //     Quantity = 1
                // };

                // // 4. Lưu ProjectResource và lấy ID
                // var resourceId = await _projectRepository.AddResourceAsync(projectResource);
                var existingResource = await _projectRepository.GetResourceByNameAndProjectId(documentFile.FileName, project.ProjectId);
            
                int resourceId;
                if (existingResource == null)
                {
                // Tạo ProjectResource mới nếu chưa tồn tại
                    var projectResource = new ProjectResource
                    {
                        ResourceName = documentFile.FileName,
                        ResourceType = 1, // Loại resource là Document
                        ProjectId = project.ProjectId,
                        Acquired = true,
                        Quantity = 1
                    };
                
                    resourceId = await _projectRepository.AddResourceAsync(projectResource);
                }
                else
                {
                // Sử dụng ID của resource đã tồn tại
                resourceId = existingResource.ProjectResourceId;
                }
                // 5. Tạo document record với ProjectResourceId vừa tạo
                var document = new Document
                {
                    ProjectId = project.ProjectId,
                    DocumentUrl = documentUrl,
                    FileName = documentFile.FileName,
                    DocumentType = (int)DocumentTypeEnum.ProjectProposal,
                    UploadAt = DateTime.Now,
                    UploadedBy = createdBy,
                    ProjectResourceId = resourceId,
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
