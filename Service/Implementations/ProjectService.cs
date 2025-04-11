using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Domain.DTO.Responses;
using Microsoft.EntityFrameworkCore;

namespace Service.Implementations;
public class ProjectService : IProjectService
{
    private readonly IS3Service _s3Service;
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,
        IMilestoneRepository milestoneRepository, INotificationService notificationService, IMapper mapper, LRMSDbContext context)
    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _milestoneRepository = milestoneRepository;
        _notificationService = notificationService;
        _mapper = mapper;
        _context = context;
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
            // Kiểm tra thời gian đăng ký
            var isValidTime = await _timelineService.IsValidTimeForAction(
                TimelineTypes.RegisterProject,
                request.SequenceId
            );

            if (!isValidTime)
                throw new ServiceException("Out of time for project registration");

            var existingProjects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            if (existingProjects.Any(p => p.ProjectName.Equals(request.ProjectName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ServiceException($"'{request.ProjectName}' has already exist. Please choose a different name.");
            }
            
            // Find the current active registration timeline
            var currentDate = DateTime.Now;
            var activeRegistrationTimeline = await _context.Timelines
                .Include(t => t.Sequence)
                .Where(t => t.TimelineType == 1 && // Assuming 1 is registration type
                       t.StartDate <= currentDate &&
                       t.EndDate >= currentDate)
                .FirstOrDefaultAsync();
            
            if (activeRegistrationTimeline == null)
            {
                throw new ServiceException("Project registration is not currently open. Please check the registration schedule.");
            }
            
            // Use the sequence ID from the active timeline
            int sequenceId = activeRegistrationTimeline.SequenceId.Value;
            
            // Create project with the automatically determined sequence ID
            var project = new Project
            {
                ProjectName = request.ProjectName,
                Description = request.Description,
                Methodlogy = request.Methodology,
                StartDate = request.StartDate?.Date,
                EndDate = request.EndDate?.Date,
                ApprovedBudget = request.ApprovedBudget,
                SpentBudget = 0, // Initialize with 0
                Status = (int)ProjectStatusEnum.Pending,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                GroupId = request.GroupId,
                DepartmentId = request.DepartmentId,
                ProjectType = (int)ProjectTypeEnum.Research,
                SequenceId = sequenceId // Set the automatically determined sequence ID
            };

            await _projectRepository.AddAsync(project);
            
            // Tạo các milestone từ danh sách milestones trong request
            if (request.Milestones != null && request.Milestones.Any())
            {
                foreach (var milestoneRequest in request.Milestones)
                {
                    var milestoneStartDate = milestoneRequest.StartDate.Date;
                    var milestoneEndDate = milestoneRequest.EndDate.Date;
                    
                    if (milestoneStartDate < project.StartDate || milestoneEndDate > project.EndDate)
                    {
                        throw new ServiceException("Milestone dates must be within project start and end dates.");
                    }
                    
                    var milestone = new Milestone
                    {
                        Title = milestoneRequest.Title,
                        Description = milestoneRequest.Title,
                        StartDate = milestoneStartDate,
                        EndDate = milestoneEndDate,
                        Status = (int)MilestoneStatusEnum.In_progress,
                        ProjectId = project.ProjectId,
                        AssignBy = createdBy
                    };
                    
                    try
                    {
                        await _milestoneRepository.AddMilestoneAsync(milestone);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating milestone: {ex.Message}");
                        throw new ServiceException($"Error creating milestone: {ex.Message}");
                    }
                }
            }
            
            if (documentFile != null)
            {
                var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{project.ProjectId}/documents");
                
                // Create ProjectResource for document
                var projectResource = new ProjectResource
                {
                    ResourceName = documentFile.FileName,
                    ResourceType = 1, // Document type
                    ProjectId = project.ProjectId,
                    Acquired = true,
                    Quantity = 1
                };
                
                await _context.ProjectResources.AddAsync(projectResource);
                await _context.SaveChangesAsync();
                
                // Create document with the resource
                var document = new Document
                {
                    ProjectId = project.ProjectId,
                    DocumentUrl = documentUrl,
                    FileName = documentFile.FileName,
                    DocumentType = (int)DocumentTypeEnum.ProjectProposal,
                    UploadAt = DateTime.Now,
                    UploadedBy = createdBy,
                    ProjectResourceId = projectResource.ProjectResourceId,
                    // Since the conference_expense_id is non-nullable, we need to provide a dummy value
                    // This needs database design modification ideally
                    // ConferenceExpenseId = 1 // This needs to be addressed with a database structure change
                };

                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
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
        // Kiểm tra thời gian gửi phê duyệt
        var isValidTime = await _timelineService.IsValidTimeForAction(
            TimelineTypes.ReviewProject,
            request.SequenceId
        );

        if (!isValidTime)
            throw new ServiceException("Out of time for project review submission");

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
