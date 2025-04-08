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
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,
        IMilestoneRepository milestoneRepository, INotificationService notificationService, IMapper mapper)
    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _milestoneRepository = milestoneRepository;
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
        // Tạo các milestone từ danh sách milestones trong request
            foreach (var milestoneRequest in request.Milestones)
            {
                var milestone = new Milestone
                {
                    Title = milestoneRequest.Title,
                    //Description = milestoneRequest.Title,
                    StartDate = milestoneRequest.StartDate,
                    EndDate = milestoneRequest.EndDate,
                    Status = (int)MilestoneStatusEnum.In_progress,
                    ProjectId = project.ProjectId,
                    AssignBy = createdBy
                };
                await _milestoneRepository.AddMilestoneAsync(milestone);
            }
        if (documentFile != null)
            {
                var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{project.ProjectId}/documents");
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

    public async Task<bool> ApproveProjectBySecretary(int projectId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            if (documentFile == null)
                throw new ServiceException("Vui lòng tải lên biên bản họp hội đồng");
            // Lấy thông tin project
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Không tìm thấy project");

            // Kiểm tra project có đang ở trạng thái chờ phê duyệt không
            if (project.Status != (int)ProjectStatusEnum.Pending)
                throw new ServiceException("Project không ở trạng thái chờ phê duyệt");

            // Lấy thông tin người phê duyệt
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null)
                throw new ServiceException("Không tìm thấy thông tin người phê duyệt");

            // Kiểm tra người phê duyệt có phải là thư ký hội đồng không
            if (secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền phê duyệt project");

            // Kiểm tra thư ký có cùng department với project không
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với project này");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{projectId}/council-documents");
            
            // Tạo ProjectResource cho document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

            // Tạo Document record
            var document = new Document
            {
                ProjectId = projectId,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = resourceId
            };
            await _projectRepository.AddDocumentAsync(document);

            // Cập nhật trạng thái project
            project.Status = (int)ProjectStatusEnum.Approved;
            await _projectRepository.UpdateAsync(project);

            // Gửi thông báo cho nhóm nghiên cứu
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project đã được phê duyệt",
                    Message = $"Project '{project.ProjectName}' đã được phê duyệt bởi thư ký hội đồng",
                    ProjectId = project.ProjectId
                };
                await _notificationService.CreateNotification(notificationRequest);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Lỗi khi phê duyệt project: {ex.Message}");
        }
    }

    public async Task<bool> RejectProjectBySecretary(int projectId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            if (documentFile == null)
                throw new ServiceException("Vui lòng tải lên biên bản họp hội đồng");

            // Lấy thông tin project
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Không tìm thấy project");

            // Kiểm tra project có đang ở trạng thái chờ phê duyệt không
            if (project.Status != (int)ProjectStatusEnum.Pending)
                throw new ServiceException("Project không ở trạng thái chờ phê duyệt");

            // Lấy thông tin người phê duyệt
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null)
                throw new ServiceException("Không tìm thấy thông tin người phê duyệt");

            // Kiểm tra người phê duyệt có phải là thư ký hội đồng không
            if (secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền từ chối project");

            // Kiểm tra thư ký có cùng department với project không
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với project này");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{projectId}/council-documents");
            
            // Tạo ProjectResource cho document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

            // Tạo Document record
            var document = new Document
            {
                ProjectId = projectId,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = resourceId
            };
            await _projectRepository.AddDocumentAsync(document);

            // Cập nhật trạng thái project
            project.Status = (int)ProjectStatusEnum.Rejected;
            await _projectRepository.UpdateAsync(project);

            // Gửi thông báo cho nhóm nghiên cứu
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project đã bị từ chối",
                    Message = $"Project '{project.ProjectName}' đã bị từ chối bởi hội đồng",
                    ProjectId = project.ProjectId
                };
                await _notificationService.CreateNotification(notificationRequest);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Lỗi khi từ chối project: {ex.Message}");
        }
    }
}
