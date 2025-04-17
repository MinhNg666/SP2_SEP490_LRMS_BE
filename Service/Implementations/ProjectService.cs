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
    private readonly IProjectPhaseRepository _projectPhaseRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;


    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,

        IProjectPhaseRepository projectPhaseRepository, INotificationService notificationService, IMapper mapper, LRMSDbContext context)

    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _projectPhaseRepository = projectPhaseRepository;
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
            

            var existingProjects = await _projectRepository.GetAllProjectsWithDetailsAsync();
            if (existingProjects.Any(p => p.ProjectName.Equals(request.ProjectName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ServiceException($"'{request.ProjectName}' has already exist. Please choose a different name.");
            }
            
            // Find the current active registration timeline
            var currentDate = DateTime.Now;
            var activeRegistrationTimeline = await _context.Timelines
                .Include(t => t.Sequence)
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ProjectRegistration &&
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
                SpentBudget = 0, 
            Status = (int)ProjectStatusEnum.Pending,
            CreatedAt = DateTime.Now,
            CreatedBy = createdBy,
            GroupId = request.GroupId,
            DepartmentId = request.DepartmentId,
                ProjectType = (int)ProjectTypeEnum.Research,
                SequenceId = sequenceId // Set the automatically determined sequence ID
        };

        await _projectRepository.AddAsync(project);
            
            // Modified project phase creation code with detailed error handling
            if (request.ProjectPhases != null && request.ProjectPhases.Any())
            {
                Console.WriteLine($"Processing {request.ProjectPhases.Count} project phases for project {project.ProjectId}");
                
                foreach (var phaseRequest in request.ProjectPhases)
                {
                    try
                    {
                        var phaseStartDate = phaseRequest.StartDate.Date;
                        var phaseEndDate = phaseRequest.EndDate.Date;
                        
                        Console.WriteLine($"Creating project phase: {phaseRequest.Title} ({phaseStartDate:yyyy-MM-dd} to {phaseEndDate:yyyy-MM-dd})");
                        
                        // Validate phase dates against project dates
                        if (project.StartDate.HasValue && project.EndDate.HasValue && 
                            (phaseStartDate < project.StartDate || phaseEndDate > project.EndDate))
                        {
                            throw new ServiceException($"Project phase dates ({phaseStartDate:yyyy-MM-dd} to {phaseEndDate:yyyy-MM-dd}) must be within project start and end dates ({project.StartDate?.Date:yyyy-MM-dd} to {project.EndDate?.Date:yyyy-MM-dd}).");
                        }
                        
                        // Create a new project phase
                        var projectPhase = new ProjectPhase
                        {
                            Title = phaseRequest.Title,
                            Description = phaseRequest.Title, // Using title as description
                            StartDate = phaseStartDate,
                            EndDate = phaseEndDate,
                            Status = (int)ProjectPhaseStatusEnum.In_progress,
                    ProjectId = project.ProjectId,
                            AssignBy = createdBy,
                            // These are optional fields based on your schema
                            AssignTo = null
                        };
                        
                        // Try two different approaches to insert the project phase
                        try
                        {
                            // Method 1: Use the ProjectPhaseRepository
                            await _projectPhaseRepository.AddProjectPhaseAsync(projectPhase);
                            Console.WriteLine("Successfully added project phase using repository");
                        }
                        catch (Exception repoEx)
                        {
                            Console.WriteLine($"Error using repository to add project phase: {repoEx.Message}");
                            
                            // Method 2: Try using DbContext directly
                            try
                            {
                                _context.ProjectPhases.Add(projectPhase);
                                await _context.SaveChangesAsync();
                                Console.WriteLine("Successfully added project phase using direct DbContext");
                            }
                            catch (Exception dbEx)
                            {
                                Console.WriteLine($"Error using direct DbContext to add project phase: {dbEx.Message}");
                                
                                // Method 3: Try inserting using raw SQL
                                try
                                {
                                    var sql = @"
                                        INSERT INTO [dbo].[ProjectPhase] (
                                            [title], [description], [start_date], [end_date], 
                                            [status], [assign_by], [project_id]
                                        ) VALUES (
                                            @title, @description, @startDate, @endDate,
                                            @status, @assignBy, @projectId
                                        )";
                                    
                                    var parameters = new[]
                                    {
                                        new SqlParameter("@title", projectPhase.Title ?? (object)DBNull.Value),
                                        new SqlParameter("@description", projectPhase.Description ?? (object)DBNull.Value),
                                        new SqlParameter("@startDate", projectPhase.StartDate ?? (object)DBNull.Value),
                                        new SqlParameter("@endDate", projectPhase.EndDate ?? (object)DBNull.Value),
                                        new SqlParameter("@status", projectPhase.Status ?? (object)DBNull.Value),
                                        new SqlParameter("@assignBy", projectPhase.AssignBy ?? (object)DBNull.Value),
                                        new SqlParameter("@projectId", projectPhase.ProjectId ?? (object)DBNull.Value)
                                    };
                                    
                                    await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                                    Console.WriteLine("Successfully added project phase using raw SQL");
                                }
                                catch (Exception sqlEx)
                                {
                                    Console.WriteLine($"Error using raw SQL to add project phase: {sqlEx.Message}");
                                    // At this point, all three methods have failed
                                    throw new ServiceException($"Failed to add project phase after trying multiple methods: {sqlEx.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing project phase {phaseRequest.Title}: {ex.Message}");
                        // Continue with other phases even if this one fails
                    }
                }
            }
            else
            {
                Console.WriteLine("No project phases to create - request.ProjectPhases is null or empty");
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

                throw new ServiceException("Please upload the council meeting minutes document");

            // Timeline validation code remains the same
            var currentDate = DateTime.Now.Date;
            var allReviewTimelines = await _context.Timelines
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod)
                .ToListAsync();

            // Console logging code remains the same
            
            var activeReviewTimeline = await _context.Timelines
                .Include(t => t.Sequence)
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod && 
                       t.StartDate <= currentDate &&
                       t.EndDate >= currentDate)
                .FirstOrDefaultAsync();
            
            if (activeReviewTimeline == null)
            {
                throw new ServiceException("Project review is not currently open. Please check the review schedule.");
            }

            // Get project information
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");


            // Check if project is in pending status
            if (project.Status != (int)ProjectStatusEnum.Pending)
                throw new ServiceException("Project is not in pending status");

            // Get approver information
            var approver = await _groupRepository.GetMemberByUserId(secretaryId);
            if (approver == null)
                throw new ServiceException("Approver information not found");

            // Check if approver is either a Secretary or Council Chairman
            if (approver.Role != (int)GroupMemberRoleEnum.Secretary && 
                approver.Role != (int)GroupMemberRoleEnum.Council_Chairman)
                throw new ServiceException("You don't have permission to approve this project. Only the council secretary or chairman can approve projects.");

            // Check if approver belongs to the same department as the project
            var approverGroup = await _groupRepository.GetByIdAsync(approver.GroupId.Value);
            if (approverGroup.GroupDepartment != project.DepartmentId)
                throw new ServiceException("You don't belong to the same department as this project");

            // Upload document and create records - remains the same
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{projectId}/council-documents");
            
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

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


            // Update project status
            project.Status = (int)ProjectStatusEnum.Approved;
            project.ApprovedBy = approver.GroupMemberId; // Store the approver
            await _projectRepository.UpdateAsync(project);

            // Get approver role name for notification
            string approverRoleName = approver.Role == (int)GroupMemberRoleEnum.Secretary ? 
                "council secretary" : "council chairman";

            // Send notifications to research group members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Approved",
                    Message = $"Project '{project.ProjectName}' has been approved by the {approverRoleName}",
                    ProjectId = project.ProjectId
                };
                await _notificationService.CreateNotification(notificationRequest);
            }

            // Create a new quota with the same budget as the project
            var quota = new Quota
            {
                AllocatedBudget = project.ApprovedBudget,
                Status = (int)QuotaStatusEnum.Active,
                CreatedAt = DateTime.Now,
                ProjectId = projectId,
                AllocatedBy = secretaryId
            };

            await _context.Quotas.AddAsync(quota);
            await _context.SaveChangesAsync();

            // debug
            Console.WriteLine($"Created quota {quota.QuotaId} for project {projectId} with budget {quota.AllocatedBudget}");

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error while approving project: {ex.Message}");
        }
    }

    public async Task<bool> RejectProjectBySecretary(int projectId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            if (documentFile == null)
                throw new ServiceException("Please upload the council meeting minutes document");


            // Timeline validation code remains the same
            var currentDate = DateTime.Now.Date;
            var allReviewTimelines = await _context.Timelines
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod)
                .ToListAsync();

            // Console logging code remains the same
            
            var activeReviewTimeline = await _context.Timelines
                .Include(t => t.Sequence)
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod && 
                       t.StartDate <= currentDate &&
                       t.EndDate >= currentDate)
                .FirstOrDefaultAsync();
            
            if (activeReviewTimeline == null)
            {
                throw new ServiceException("Project review is not currently open. Please check the review schedule.");
            }

            // Get project information
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");


            // Check if project is in pending status
            if (project.Status != (int)ProjectStatusEnum.Pending)
                throw new ServiceException("Project is not in pending status");

            // Get approver information
            var approver = await _groupRepository.GetMemberByUserId(secretaryId);
            if (approver == null)
                throw new ServiceException("Approver information not found");

            // Check if approver is either a Secretary or Council Chairman
            if (approver.Role != (int)GroupMemberRoleEnum.Secretary && 
                approver.Role != (int)GroupMemberRoleEnum.Council_Chairman)
                throw new ServiceException("You don't have permission to reject this project. Only the council secretary or chairman can reject projects.");

            // Check if approver belongs to the same department as the project
            var approverGroup = await _groupRepository.GetByIdAsync(approver.GroupId.Value);
            if (approverGroup.GroupDepartment != project.DepartmentId)
                throw new ServiceException("You don't belong to the same department as this project");

            // Upload document and create records - remains the same
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{projectId}/council-documents");
            
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

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

            // Update project status
            project.Status = (int)ProjectStatusEnum.Rejected;
            project.ApprovedBy = approver.GroupMemberId; // Store the approver
            await _projectRepository.UpdateAsync(project);

            // Get approver role name for notification
            string approverRoleName = approver.Role == (int)GroupMemberRoleEnum.Secretary ? 
                "council secretary" : "council chairman";

            // Send notifications to research group members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Rejected",
                    Message = $"Project '{project.ProjectName}' has been rejected by the {approverRoleName}",
                    ProjectId = project.ProjectId
                };
                await _notificationService.CreateNotification(notificationRequest);
            }


            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error while rejecting project: {ex.Message}");
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
                .Include(p => p.ProjectPhases)
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
                ProjectPhases = projectResponse.ProjectPhases,
                SpentBudget = projectResponse.SpentBudget,
                
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

    public async Task<bool> UpdateProjectPhaseStatus(int projectPhaseId, int status, int userId)
    {
        try
        {
            // Validate the status is in the enum range
            if (!Enum.IsDefined(typeof(ProjectPhaseStatusEnum), status))
                throw new ServiceException($"Invalid status value: {status}");
            
            // Get the project phase
            var projectPhase = await _context.ProjectPhases
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.ProjectPhaseId == projectPhaseId);
            
            if (projectPhase == null)
                throw new ServiceException("Project phase not found");
            
            // Get the project
            var project = projectPhase.Project;
            if (project == null)
                throw new ServiceException("Project not found for this phase");
            
            // Check if user is a member of the project group
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            var groupIds = userGroups.Select(g => g.GroupId);
            
            if (!project.GroupId.HasValue || !groupIds.Contains(project.GroupId.Value))
                throw new ServiceException("You don't have permission to update this project phase");
            
            // Check if the project is approved (can't modify phases of pending or rejected projects)
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Cannot update phases of projects that are not approved");
            
            // Update the status
            var result = await _projectPhaseRepository.UpdateProjectPhaseStatusAsync(projectPhaseId, status);
            if (!result)
                throw new ServiceException("Failed to update project phase status");
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating project phase status: {ex.Message}");
        }
    }

    public async Task UpdateProjectPhaseStatusesBasedOnDates()
    {
        try
        {
            await _projectPhaseRepository.UpdateProjectPhaseStatusesBasedOnDates();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating project phase statuses based on dates: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateProjectPhase(int projectPhaseId, int status, decimal spentBudget, DateTime? startDate, DateTime? endDate, string title, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate the status is in the enum range
            if (!Enum.IsDefined(typeof(ProjectPhaseStatusEnum), status))
                throw new ServiceException($"Invalid status value: {status}");
            
            // Get the project phase
            var projectPhase = await _context.ProjectPhases
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.ProjectPhaseId == projectPhaseId);
            
            if (projectPhase == null)
                throw new ServiceException("Project phase not found");
            
            // Get the project
            var project = projectPhase.Project;
            if (project == null)
                throw new ServiceException("Project not found for this phase");
            
            // Check if user is a member of the project group
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            var groupIds = userGroups.Select(g => g.GroupId);
            
            if (!project.GroupId.HasValue || !groupIds.Contains(project.GroupId.Value))
                throw new ServiceException("You don't have permission to update this project phase");
            
            // Check if the project is approved (can't modify phases of pending or rejected projects)
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Cannot update phases of projects that are not approved");
            
            // Prevent ALL updates to completed project phases
            if (projectPhase.Status == (int)ProjectPhaseStatusEnum.Completed)
                throw new ServiceException("Cannot update a completed project phase");
            
            // Save the old spent budget for later calculation
            decimal oldSpentBudget = projectPhase.SpentBudget;
            
            // Always update status and spent budget (these can be modified regardless of current status)
            projectPhase.Status = status;
            projectPhase.SpentBudget = spentBudget;
            
            // Only update title and dates if the phase is not being set to Completed
            if (status != (int)ProjectPhaseStatusEnum.Completed)
            {
                // Update title if provided and not empty
                if (!string.IsNullOrWhiteSpace(title))
                {
                    projectPhase.Title = title;
                    // Update description to match title if that's your business logic
                    projectPhase.Description = title;
                }
                
                // Only update dates if project phase is in Pending status
                if (projectPhase.Status == (int)ProjectPhaseStatusEnum.Pending)
                {
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        // Validate dates
                        if (startDate.Value > endDate.Value)
                            throw new ServiceException("Start date cannot be after end date");
                        
                        // Validate dates against project dates
                        if (project.StartDate.HasValue && project.EndDate.HasValue &&
                            (startDate.Value < project.StartDate.Value || endDate.Value > project.EndDate.Value))
                            throw new ServiceException($"Project phase dates must be within project dates ({project.StartDate.Value:yyyy-MM-dd} to {project.EndDate.Value:yyyy-MM-dd})");
                        
                        projectPhase.StartDate = startDate.Value;
                        projectPhase.EndDate = endDate.Value;
                    }
                    else if (startDate.HasValue || endDate.HasValue)
                    {
                        throw new ServiceException("Both start date and end date must be provided to update dates");
                    }
                }
                else if ((startDate.HasValue || endDate.HasValue) && projectPhase.Status != (int)ProjectPhaseStatusEnum.Pending)
                {
                    throw new ServiceException("Dates can only be updated for pending project phases");
                }
            }
            
            // Save the project phase first
            _context.ProjectPhases.Update(projectPhase);
            await _context.SaveChangesAsync();
            
            // Now recalculate the project's total spent budget directly
            var projectId = project.ProjectId;
            var totalSpent = await _context.ProjectPhases
                .Where(p => p.ProjectId == projectId)
                .SumAsync(p => p.SpentBudget);
            
            // Update the project's spent budget
            project.SpentBudget = totalSpent;
            project.UpdatedAt = DateTime.Now;
            
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error updating project phase: {ex.Message}");
        }
    }
}
