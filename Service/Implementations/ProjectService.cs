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
using Microsoft.Data.SqlClient;

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
    private readonly ITimelineService _timelineService;

    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,
        IMilestoneRepository milestoneRepository, INotificationService notificationService, IMapper mapper, LRMSDbContext context, ITimelineService timelineService)
    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _milestoneRepository = milestoneRepository;
        _notificationService = notificationService;
        _mapper = mapper;
        _context = context;
        _timelineService = timelineService;
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
            
            // Modified milestone creation code with detailed error handling
            if (request.Milestones != null && request.Milestones.Any())
            {
                Console.WriteLine($"Processing {request.Milestones.Count} milestones for project {project.ProjectId}");
                
                foreach (var milestoneRequest in request.Milestones)
                {
                    try
                    {
                        var milestoneStartDate = milestoneRequest.StartDate.Date;
                        var milestoneEndDate = milestoneRequest.EndDate.Date;
                        
                        Console.WriteLine($"Creating milestone: {milestoneRequest.Title} ({milestoneStartDate:yyyy-MM-dd} to {milestoneEndDate:yyyy-MM-dd})");
                        
                        // Validate milestone dates against project dates
                        if (project.StartDate.HasValue && project.EndDate.HasValue && 
                            (milestoneStartDate < project.StartDate || milestoneEndDate > project.EndDate))
                        {
                            throw new ServiceException($"Milestone dates ({milestoneStartDate:yyyy-MM-dd} to {milestoneEndDate:yyyy-MM-dd}) must be within project start and end dates ({project.StartDate?.Date:yyyy-MM-dd} to {project.EndDate?.Date:yyyy-MM-dd}).");
                        }
                        
                        // Create a new milestone
                        var milestone = new Milestone
                        {
                            Title = milestoneRequest.Title,
                            Description = milestoneRequest.Title, // Using title as description
                            StartDate = milestoneStartDate,
                            EndDate = milestoneEndDate,
                            Status = (int)MilestoneStatusEnum.In_progress,
                            ProjectId = project.ProjectId,
                            AssignBy = createdBy,
                            // These are optional fields based on your schema
                            AssignTo = null
                        };
                        
                        // Try two different approaches to insert the milestone
                        try
                        {
                            // Method 1: Use the MilestoneRepository
                            await _milestoneRepository.AddMilestoneAsync(milestone);
                            Console.WriteLine("Successfully added milestone using repository");
                        }
                        catch (Exception repoEx)
                        {
                            Console.WriteLine($"Error using repository to add milestone: {repoEx.Message}");
                            
                            // Method 2: Try using DbContext directly
                            try
                            {
                                _context.Milestones.Add(milestone);
                                await _context.SaveChangesAsync();
                                Console.WriteLine("Successfully added milestone using direct DbContext");
                            }
                            catch (Exception dbEx)
                            {
                                Console.WriteLine($"Error using direct DbContext to add milestone: {dbEx.Message}");
                                
                                // Method 3: Try inserting using raw SQL
                                try
                                {
                                    var sql = @"
                                        INSERT INTO [dbo].[Milestone] (
                                            [title], [description], [start_date], [end_date], 
                                            [status], [assign_by], [project_id]
                                        ) VALUES (
                                            @title, @description, @startDate, @endDate,
                                            @status, @assignBy, @projectId
                                        )";
                                    
                                    var parameters = new[]
                                    {
                                        new SqlParameter("@title", milestone.Title ?? (object)DBNull.Value),
                                        new SqlParameter("@description", milestone.Description ?? (object)DBNull.Value),
                                        new SqlParameter("@startDate", milestone.StartDate ?? (object)DBNull.Value),
                                        new SqlParameter("@endDate", milestone.EndDate ?? (object)DBNull.Value),
                                        new SqlParameter("@status", milestone.Status ?? (object)DBNull.Value),
                                        new SqlParameter("@assignBy", milestone.AssignBy ?? (object)DBNull.Value),
                                        new SqlParameter("@projectId", milestone.ProjectId ?? (object)DBNull.Value)
                                    };
                                    
                                    await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                                    Console.WriteLine("Successfully added milestone using raw SQL");
                                }
                                catch (Exception sqlEx)
                                {
                                    Console.WriteLine($"Error using raw SQL to add milestone: {sqlEx.Message}");
                                    // At this point, all three methods have failed
                                    throw new ServiceException($"Failed to add milestone after trying multiple methods: {sqlEx.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing milestone {milestoneRequest.Title}: {ex.Message}");
                        // Continue with other milestones even if this one fails
                    }
                }
            }
            else
            {
                Console.WriteLine("No milestones to create - request.Milestones is null or empty");
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
                    // ConferenceExpenseId = 1 // This needs to be addressed with a database structure change
                };

                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
            }
            
        return project.ProjectId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateResearchProject: {ex.Message}");
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

    public async Task AddProjectDocument(int projectId, IFormFile documentFile, int userId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");
            
            if (documentFile != null)
            {
                var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{projectId}/documents");
                
                // Create ProjectResource for document
                var projectResource = new ProjectResource
                {
                    ResourceName = documentFile.FileName,
                    ResourceType = 1, // Document type
                    ProjectId = projectId,
                    Acquired = true,
                    Quantity = 1
                };
                
                await _context.ProjectResources.AddAsync(projectResource);
                await _context.SaveChangesAsync();
                
                // Create document with the resource
                var document = new Document
                {
                    ProjectId = projectId,
                    DocumentUrl = documentUrl,
                    FileName = documentFile.FileName,
                    DocumentType = (int)DocumentTypeEnum.ProjectProposal,
                    UploadAt = DateTime.Now,
                    UploadedBy = userId,
                    ProjectResourceId = projectResource.ProjectResourceId
                };

                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error adding document to project: {ex.Message}");
        }
    }

    public async Task<ProjectDetailResponse> GetProjectDetails(int projectId)
    {
        try
        {
            // Get project with all related entities
            var project = await _context.Projects
                .Include(p => p.Documents)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupDepartmentNavigation)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupMembers)
                        .ThenInclude(gm => gm.User)
                .Include(p => p.Department)
                .Include(p => p.Milestones)
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.ApprovedByNavigation)
                    .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException("Project not found");

            // Map to base ProjectResponse
            var projectResponse = _mapper.Map<ProjectResponse>(project);
            
            // Create and populate the detailed response
            var detailedResponse = new ProjectDetailResponse
            {
                // Copy all base properties
                ProjectId = projectResponse.ProjectId,
                ProjectName = projectResponse.ProjectName,
                ProjectType = projectResponse.ProjectType,
                Description = projectResponse.Description,
                ApprovedBudget = projectResponse.ApprovedBudget,
                Status = projectResponse.Status,
                StartDate = projectResponse.StartDate,
                EndDate = projectResponse.EndDate,
                CreatedAt = projectResponse.CreatedAt,
                UpdatedAt = projectResponse.UpdatedAt,
                Methodology = projectResponse.Methodology,
                CreatedBy = projectResponse.CreatedBy,
                ApprovedBy = projectResponse.ApprovedBy,
                GroupId = projectResponse.GroupId,
                GroupName = projectResponse.GroupName,
                DepartmentId = projectResponse.DepartmentId,
                Documents = projectResponse.Documents,
                Milestones = projectResponse.Milestones,
                
                // Add enhanced creator info
                CreatedByUser = project.CreatedByNavigation != null ? new UserShortInfo
                {
                    UserId = project.CreatedByNavigation.UserId,
                    Username = project.CreatedByNavigation.Username,
                    FullName = project.CreatedByNavigation.FullName,
                    Email = project.CreatedByNavigation.Email
                } : null,
                
                // Add enhanced approver info
                ApprovedByUser = project.ApprovedByNavigation?.User != null ? new UserShortInfo
                {
                    UserId = project.ApprovedByNavigation.User.UserId,
                    Username = project.ApprovedByNavigation.User.Username,
                    FullName = project.ApprovedByNavigation.User.FullName,
                    Email = project.ApprovedByNavigation.User.Email
                } : null,
                
                // Add enhanced group info
                Group = project.Group != null ? new GroupDetailInfo
                {
                    GroupId = project.Group.GroupId,
                    GroupName = project.Group.GroupName,
                    GroupType = project.Group.GroupType ?? 0,
                    CurrentMember = project.Group.CurrentMember ?? 0,
                    MaxMember = project.Group.MaxMember ?? 0,
                    GroupDepartment = project.Group.GroupDepartment,
                    DepartmentName = project.Group?.GroupDepartmentNavigation?.DepartmentName,
                    Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(
                        project.Group.GroupMembers.Where(gm => gm.Status == (int)GroupMemberStatus.Active))
                } : null,
                
                // Add enhanced department info
                Department = project.Department != null ? new DepartmentResponse
                {
                    DepartmentId = project.Department.DepartmentId,
                    DepartmentName = project.Department.DepartmentName
                } : null
            };
            
            return detailedResponse;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting project details: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectListResponse>> GetUserPendingProjectsList(int userId)
    {
        try
        {
            // Get user's groups
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            if (!userGroups.Any())
            {
                return Enumerable.Empty<ProjectListResponse>();
            }

            // Get projects for user's groups with Pending status
            var groupIds = userGroups.Select(g => g.GroupId);
            var projects = await _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Group)
                .Include(p => p.Department)
                .Where(p => 
                    p.GroupId.HasValue && 
                    groupIds.Contains(p.GroupId.Value) && 
                    p.Status == (int)ProjectStatusEnum.Pending)
                .ToListAsync();

            return projects.Select(p => new ProjectListResponse
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                Status = p.Status,
                ApprovedBudget = Convert.ToDouble(p.ApprovedBudget ?? 0),
                StartDate = p.StartDate ?? DateTime.MinValue,
                EndDate = p.EndDate ?? DateTime.MinValue,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                CreatedBy = p.CreatedBy ?? 0,
                CreatorName = p.CreatedByNavigation?.FullName ?? "Unknown",
                CreatorEmail = p.CreatedByNavigation?.Email ?? "Unknown",
                GroupId = p.GroupId,
                GroupName = p.Group?.GroupName ?? "Unknown",
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department?.DepartmentName ?? "Unknown"
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting user's pending projects: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectListResponse>> GetUserApprovedProjectsList(int userId)
    {
        try
        {
            // Get user's groups
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            if (!userGroups.Any())
            {
                return Enumerable.Empty<ProjectListResponse>();
            }

            // Get projects for user's groups with Approved status
            var groupIds = userGroups.Select(g => g.GroupId);
            var projects = await _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Group)
                .Include(p => p.Department)
                .Where(p => 
                    p.GroupId.HasValue && 
                    groupIds.Contains(p.GroupId.Value) && 
                    p.Status == (int)ProjectStatusEnum.Approved)
                .ToListAsync();

            return projects.Select(p => new ProjectListResponse
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Description = p.Description,
                Status = p.Status,
                ApprovedBudget = Convert.ToDouble(p.ApprovedBudget ?? 0),
                StartDate = p.StartDate ?? DateTime.MinValue,
                EndDate = p.EndDate ?? DateTime.MinValue,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                CreatedBy = p.CreatedBy ?? 0,
                CreatorName = p.CreatedByNavigation?.FullName ?? "Unknown",
                CreatorEmail = p.CreatedByNavigation?.Email ?? "Unknown",
                GroupId = p.GroupId,
                GroupName = p.Group?.GroupName ?? "Unknown",
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department?.DepartmentName ?? "Unknown"
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting user's approved projects: {ex.Message}");
        }
    }
}
