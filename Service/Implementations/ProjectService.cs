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
using System.Text;

namespace Service.Implementations;
public class ProjectService : IProjectService
{
    private readonly IS3Service _s3Service;
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IProjectPhaseRepository _projectPhaseRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IGroupService _groupService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;


    public ProjectService(IS3Service s3Service, IProjectRepository projectRepository, IGroupRepository groupRepository,
        IDepartmentRepository departmentRepository, IUserRepository userRepository, IGroupService groupService, IEmailService emailService,
        IProjectPhaseRepository projectPhaseRepository, INotificationService notificationService, IMapper mapper, LRMSDbContext context)

    {
        _s3Service = s3Service;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _projectPhaseRepository = projectPhaseRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _groupService = groupService;
        _emailService = emailService;
        _mapper = mapper;
        _context = context;

    }

    private string GetProjectTypeName(int? projectType)
    {
        if (!projectType.HasValue) return "Không xác định";
        return projectType.Value switch
        {
            (int)ProjectTypeEnum.Research => "Nghiên cứu",
            (int)ProjectTypeEnum.Conference => "Hội nghị",
            (int)ProjectTypeEnum.Journal => "Tạp chí",
            _ => "Không xác định"
        };
    }

    private async Task SendEmailToStakeholders(int groupId, string subject, string message)
    {
        try
        {
            // Lấy danh sách stakeholder từ group_member
            var stakeholders = await _context.GroupMembers
                .Include(gm => gm.User)
                .Where(gm => gm.GroupId == groupId && 
                   gm.Role == (int)GroupMemberRoleEnum.Stakeholder &&
                   gm.Status == (int)GroupMemberStatus.Active &&
                   gm.User != null)
                .Select(gm => gm.User.Email)
                .ToListAsync();

            foreach (var email in stakeholders)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    await _emailService.SendEmailAsync(email, subject, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi gửi email cho stakeholder: {ex.Message}");
        }
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
            
            // Create project with initial Pending status
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
                SequenceId = sequenceId
        };

        await _projectRepository.AddAsync(project);
            
            // Create ProjectRequest for this project creation
            var projectRequest = new ProjectRequest
            {
                ProjectId = project.ProjectId,
                RequestType = ProjectRequestTypeEnum.Research_Creation,
                RequestedById = createdBy,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatusEnum.Pending
            };
            
            await _context.ProjectRequests.AddAsync(projectRequest);
            await _context.SaveChangesAsync();
            
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
                        
                        // Determine the initial status based on dates
                        int initialStatus;
                        if (phaseStartDate > currentDate)
                        {
                            // If start date is in the future, set as Pending
                            initialStatus = (int)ProjectPhaseStatusEnum.Pending;
                        }
                        else if (phaseStartDate <= currentDate && phaseEndDate >= currentDate)
                        {
                            // If current date is between start and end dates, set as In_progress
                            initialStatus = (int)ProjectPhaseStatusEnum.In_progress;
                        }
                        else
                        {
                            // If end date is in the past, set as Overdued
                            initialStatus = (int)ProjectPhaseStatusEnum.Overdued;
                        }
                        
                        // Create a new project phase with the appropriate initial status
                        var projectPhase = new ProjectPhase
                        {
                            Title = phaseRequest.Title,
                            Description = phaseRequest.Title, // Using title as description
                            StartDate = phaseStartDate,
                            EndDate = phaseEndDate,
                            Status = initialStatus, // Set the calculated status instead of hardcoded value
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

                // Lấy thông tin chi tiết để gửi thông báo
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
                var group = await _groupRepository.GetByIdAsync(request.GroupId);
                var creator = await _userRepository.GetByIdAsync(createdBy);
                // Tạo thông tin chi tiết về project
                var projectInfo = new StringBuilder();
                projectInfo.AppendLine($"Thông tin chi tiết về project mới:");
                projectInfo.AppendLine($"- Tên project: {project.ProjectName}");
                projectInfo.AppendLine($"- Loại project: {GetProjectTypeName(project.ProjectType)}");
                projectInfo.AppendLine($"- Mô tả: {project.Description}");
                projectInfo.AppendLine($"- Phương pháp: {project.Methodlogy}");
                projectInfo.AppendLine($"- Khoa/Phòng ban: {department?.DepartmentName}");
                projectInfo.AppendLine($"- Nhóm nghiên cứu: {group?.GroupName}");
                projectInfo.AppendLine($"- Người tạo: {creator?.FullName}");
                projectInfo.AppendLine($"- Ngày bắt đầu: {project.StartDate:dd/MM/yyyy}");
                projectInfo.AppendLine($"- Ngày kết thúc: {project.EndDate:dd/MM/yyyy}");

                // Thêm thông tin về milestones nếu có
                if (request.ProjectPhases != null && request.ProjectPhases.Any())
                {
                    projectInfo.AppendLine("\nCác mốc thời gian (Milestones):");
                    foreach (var milestone in request.ProjectPhases)
                    {
                        projectInfo.AppendLine($"- {milestone.Title}:");
                        projectInfo.AppendLine($"  Bắt đầu: {milestone.StartDate:dd/MM/yyyy}");
                        projectInfo.AppendLine($"  Kết thúc: {milestone.EndDate:dd/MM/yyyy}");
                    }
                }
                // Gửi thông báo cho tất cả thành viên trong nhóm
                var groupMembers = await _groupRepository.GetMembersByGroupId(request.GroupId);
                var activeMembers = groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

                foreach (var member in activeMembers)
                {
                    if (member.UserId.HasValue)
                    {
                        // Tạo nội dung thông báo khác nhau cho người tạo và các thành viên khác
                        string title = member.UserId.Value == createdBy
                        ? "Bạn đã đăng ký project mới thành công"
                        : "Nhóm của bạn có project nghiên cứu mới";

                        string introMessage = member.UserId.Value == createdBy
                        ? "Bạn đã đăng ký thành công project mới. Project đang chờ phê duyệt từ hội đồng."
                        : $"Thành viên {creator?.FullName} đã đăng ký project mới cho nhóm. Project đang chờ phê duyệt từ hội đồng.";

                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = title,
                            Message = $"{introMessage}\n\n{projectInfo}",
                            ProjectId = project.ProjectId,
                            Status = 0,
                            IsRead = false
                        };
                        await _notificationService.CreateNotification(notificationRequest);
                    }
                }
                // Gửi email cho stakeholder
                var stakeholders = groupMembers.Where(m =>
                m.Status == (int)GroupMemberStatus.Active &&
                m.Role == (int)GroupMemberRoleEnum.Stakeholder);

                var emailSubject = $"Dự án nghiên cứu mới: {project.ProjectName}";
                var emailMessage = $"Một dự án nghiên cứu mới đã được đăng ký:\n\n{projectInfo}";

                foreach (var stakeholder in stakeholders)
                {
                    if (stakeholder.User != null)
                    {
                        await _emailService.SendEmailAsync(stakeholder.User.Email, emailSubject, emailMessage);
                    }
                }
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

    public async Task<bool> ApproveProjectBySecretary(int projectId, int secretaryId, IEnumerable<IFormFile> documentFiles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (documentFiles == null || !documentFiles.Any())
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

            // Find the corresponding ProjectRequest
            var projectRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && 
                                     r.RequestType == ProjectRequestTypeEnum.Research_Creation &&
                                     r.ApprovalStatus == ApprovalStatusEnum.Pending);
                                     
            if (projectRequest == null)
                throw new ServiceException("No pending project request found for this project");

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

            // Upload documents and create records
            var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/council-documents");
            int index = 0;
            
            foreach (var file in documentFiles)
            {
            var projectResource = new ProjectResource
            {
                    ResourceName = file.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

            var document = new Document
            {
                ProjectId = projectId,
                    DocumentUrl = urls[index],
                    FileName = file.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = resourceId
            };
            await _projectRepository.AddDocumentAsync(document);
                index++;
            }

            // First update ProjectRequest status
            projectRequest.ApprovalStatus = ApprovalStatusEnum.Approved;
            projectRequest.ApprovedById = secretaryId;
            projectRequest.ApprovedAt = DateTime.UtcNow;
            _context.ProjectRequests.Update(projectRequest);
            
            // Then update project status
            project.Status = (int)ProjectStatusEnum.Approved;
            project.ApprovedBy = approver.GroupMemberId;
            await _projectRepository.UpdateAsync(project);

            // Get approver role name for notification
            string approverRoleName = approver.Role == (int)GroupMemberRoleEnum.Secretary ?
                "council secretary" : "council chairman";

            var department = await _departmentRepository.GetByIdAsync(project.DepartmentId.Value);
            var group = await _groupRepository.GetByIdAsync(project.GroupId.Value);
            var creator = await _userRepository.GetByIdAsync(project.CreatedBy.Value);

            // Tạo thông tin chi tiết về project
            var projectInfo = new StringBuilder();
            projectInfo.AppendLine($"Thông tin chi tiết về project:");
            projectInfo.AppendLine($"- Tên project: {project.ProjectName}");
            projectInfo.AppendLine($"- Loại project: {GetProjectTypeName(project.ProjectType)}");
            projectInfo.AppendLine($"- Mô tả: {project.Description}");
            projectInfo.AppendLine($"- Phương pháp: {project.Methodlogy}");
            projectInfo.AppendLine($"- Khoa/Phòng ban: {department?.DepartmentName}");
            projectInfo.AppendLine($"- Nhóm nghiên cứu: {group?.GroupName}");
            projectInfo.AppendLine($"- Người tạo: {creator?.FullName}");
            projectInfo.AppendLine($"- Ngày bắt đầu: {project.StartDate:dd/MM/yyyy}");
            projectInfo.AppendLine($"- Ngày kết thúc: {project.EndDate:dd/MM/yyyy}");
            if (project.ApprovedBudget.HasValue)
            {
                projectInfo.AppendLine($"- Kinh phí được duyệt: {project.ApprovedBudget:N0} VNĐ");
            }
            projectInfo.AppendLine($"\nBiên bản họp hội đồng: {(urls.Any() ? urls[0] : "N/A")}");


            // Send notifications to research group members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Approved",
                    Message = $"Project '{project.ProjectName}' has been approved by the {approverRoleName}. Please see the council documents at : {(urls.Any() ? urls[0] : "N/A")}",
                    ProjectId = project.ProjectId,
                    Status = 0,
                    IsRead = false
                };
                await _notificationService.CreateNotification(notificationRequest);
            }

            // Gửi email cho stakeholder - lấy từ group members
            var stakeholders = groupMembers.Where(m => 
            m.Status == (int)GroupMemberStatus.Active && 
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            var emailSubject = $"Dự án đã được phê duyệt: {project.ProjectName}";
            var emailMessage = $"Dự án nghiên cứu đã được hội đồng phê duyệt:\n\n{projectInfo}";

            foreach (var stakeholder in stakeholders)
            {
            await _emailService.SendEmailAsync(stakeholder.User.Email, emailSubject, emailMessage);
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

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error while approving project: {ex.Message}");
        }
    }

    public async Task<bool> RejectProjectBySecretary(int projectId, int secretaryId, string rejectionReason, IEnumerable<IFormFile> documentFiles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (documentFiles == null || !documentFiles.Any())
                throw new ServiceException("Please upload the council meeting minutes document");
            
            // Timeline validation code remains the same
            var currentDate = DateTime.Now.Date;
            var allReviewTimelines = await _context.Timelines
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.ReviewPeriod)
                .ToListAsync();
            
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

            // Find the corresponding ProjectRequest
            var projectRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && 
                                     r.RequestType == ProjectRequestTypeEnum.Research_Creation &&
                                     r.ApprovalStatus == ApprovalStatusEnum.Pending);
                                     
            if (projectRequest == null)
                throw new ServiceException("No pending project request found for this project");

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

            // Upload documents and create records
            var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/council-documents");
            int index = 0;
            
            foreach (var file in documentFiles)
            {
            var projectResource = new ProjectResource
            {
                    ResourceName = file.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            var resourceId = await _projectRepository.AddResourceAsync(projectResource);

            var document = new Document
            {
                ProjectId = projectId,
                    DocumentUrl = urls[index],
                    FileName = file.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = resourceId
            };
            await _projectRepository.AddDocumentAsync(document);
                index++;
            }

            // First update ProjectRequest status
            projectRequest.ApprovalStatus = ApprovalStatusEnum.Rejected;
            projectRequest.ApprovedById = secretaryId; // Using same field even for rejection
            projectRequest.ApprovedAt = DateTime.UtcNow;
            projectRequest.RejectionReason = rejectionReason;
            _context.ProjectRequests.Update(projectRequest);
            
            // Then update project status and rejection reason
            project.Status = (int)ProjectStatusEnum.Rejected;
            project.ApprovedBy = approver.GroupMemberId;
            project.RejectionReason = rejectionReason;
            await _projectRepository.UpdateAsync(project);

            // Get approver role name for notification
            string approverRoleName = approver.Role == (int)GroupMemberRoleEnum.Secretary ? 
                "council secretary" : "council chairman";

            // Lấy thêm thông tin chi tiết
            var department = await _departmentRepository.GetByIdAsync(project.DepartmentId.Value);
            var group = await _groupRepository.GetByIdAsync(project.GroupId.Value);
            var creator = await _userRepository.GetByIdAsync(project.CreatedBy.Value);

            // Tạo thông tin chi tiết về project
            var projectInfo = new StringBuilder();
            projectInfo.AppendLine($"Thông tin chi tiết về project:");
            projectInfo.AppendLine($"- Tên project: {project.ProjectName}");
            projectInfo.AppendLine($"- Loại project: {GetProjectTypeName(project.ProjectType)}");
            projectInfo.AppendLine($"- Mô tả: {project.Description}");
            projectInfo.AppendLine($"- Phương pháp: {project.Methodlogy}");
            projectInfo.AppendLine($"- Khoa/Phòng ban: {department?.DepartmentName}");
            projectInfo.AppendLine($"- Nhóm nghiên cứu: {group?.GroupName}");
            projectInfo.AppendLine($"- Người tạo: {creator?.FullName}");
            projectInfo.AppendLine($"- Ngày bắt đầu: {project.StartDate:dd/MM/yyyy}");
            projectInfo.AppendLine($"- Ngày kết thúc: {project.EndDate:dd/MM/yyyy}");
            if (project.ApprovedBudget.HasValue)
            {
                projectInfo.AppendLine($"- Kinh phí được duyệt: {project.ApprovedBudget:N0} VNĐ");
            }
            projectInfo.AppendLine($"\nBiên bản họp hội đồng: {(urls.Any() ? urls[0] : "N/A")}");

            // Send notifications to research group members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Rejected",
                    Message = $"Project '{project.ProjectName}' has been rejected by the {approverRoleName}. Please see the council documents at : {(urls.Any() ? urls[0] : "N/A")}",
                    ProjectId = project.ProjectId,
                    Status = 0,
                    IsRead = false
                };
                await _notificationService.CreateNotification(notificationRequest);
            }
            // Gửi email cho stakeholder - lấy từ group members
            var stakeholders = groupMembers.Where(m => 
            m.Status == (int)GroupMemberStatus.Active && 
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            await transaction.CommitAsync();            var emailSubject = $"Dự án đã bị từ chối: {project.ProjectName}";
            var emailMessage = $"Dự án nghiên cứu đã bị hội đồng từ chối:\n\n{projectInfo}";

            foreach (var stakeholder in stakeholders)
            {
            await _emailService.SendEmailAsync(stakeholder.User.Email, emailSubject, emailMessage);
            }
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error while rejecting project: {ex.Message}");
        }
    }

    public async Task AddProjectDocuments(int projectId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");
            
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/documents");
                int index = 0;
                
                // Save the first file's name and URL for notifications/emails
                string firstFileName = documentFiles.First().FileName;
                string firstFileUrl = urls[0];
                
                foreach (var file in documentFiles)
                {
                    // Create ProjectResource for document
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
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
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.ProjectProposal,
                        UploadAt = DateTime.Now,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId
                    };

                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();

                // Lấy thông tin người upload
                var uploader = await _userRepository.GetByIdAsync(userId);

                // Gửi thông báo cho các thành viên trong nhóm
                var groupMembers = project.Group.GroupMembers
                    .Where(m => m.Status == (int)GroupMemberStatus.Active &&
                               m.Role != (int)GroupMemberRoleEnum.Stakeholder);

                foreach (var member in groupMembers)
                {
                    if (member.UserId.HasValue)
                    {
                        string title = member.UserId.Value == userId
                            ? "Bạn đã tải lên tài liệu mới"
                            : "Có tài liệu mới trong project";

                        string message = member.UserId.Value == userId
                            ? $"Bạn đã tải lên tài liệu mới cho project '{project.ProjectName}'"
                            : $"Thành viên {uploader?.FullName} đã tải lên tài liệu mới cho project '{project.ProjectName}'";

                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = title,
                            Message = $"{message}\n\nTên tài liệu: {firstFileName}\nXem tại: {firstFileUrl}",
                            ProjectId = projectId,
                            Status = 0,
                            IsRead = false
                        };
                        await _notificationService.CreateNotification(notificationRequest);
                    }
                }

                // Gửi email cho stakeholder
                var stakeholders = project.Group.GroupMembers
                    .Where(m => m.Status == (int)GroupMemberStatus.Active &&
                               m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
                               m.User != null);

                var emailSubject = $"Tài liệu mới trong project: {project.ProjectName}";
                var emailMessage = $"Xin chào,\n\n" +
                                  $"Một tài liệu mới đã được tải lên trong project '{project.ProjectName}':\n\n" +
                                  $"- Tên tài liệu: {firstFileName}\n" +
                                  $"- Người tải lên: {uploader?.FullName}\n" +
                                  $"- Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                                  $"- Xem tại: {firstFileUrl}\n\n" +
                                  $"Trân trọng.";

                foreach (var stakeholder in stakeholders)
                {
                    await _emailService.SendEmailAsync(
                        stakeholder.User.Email,
                        emailSubject,
                        emailMessage
                    );
                }
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error adding documents to project: {ex.Message}");
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
            throw new ServiceException($"Error adding document to project: {ex.Message}`");
        }
    }

    public async Task<bool> MarkProjectAsCompleted(int projectId, int userId)
    {
        try
        {
            // Get project with phases
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
            
            if (project == null)
                throw new ServiceException("Project not found");
            
            // Check if project is in appropriate state - NOW CHECK FOR Completion_Requested
            // Or potentially allow re-marking completed? Depends on workflow. Let's assume only requested projects.
             if (project.Status != (int)ProjectStatusEnum.Completion_Requested)
                 throw new ServiceException($"Project must be in '{ProjectStatusEnum.Completion_Requested}' status to be marked as completed. Current status: {(ProjectStatusEnum?)project.Status}");

            // Check user permissions (member of the project group) - Or should this be approver role?
            // For now, keeping original logic, but approval should likely be restricted.
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            var groupIds = userGroups.Select(g => g.GroupId);

            if (!project.GroupId.HasValue || !groupIds.Contains(project.GroupId.Value))
                 throw new ServiceException("You don't have permission to mark this project as completed."); // Modify permission check later

            // Verify all phases are completed - This check is done before requesting completion, maybe remove here?
            // if (!project.ProjectPhases.Any())
            //    throw new ServiceException("Project has no phases to complete");

            // var incompletePhasesCount = project.ProjectPhases
            //    .Count(p => p.Status != (int)ProjectPhaseStatusEnum.Completed);

            // if (incompletePhasesCount > 0)
            //    throw new ServiceException($"Cannot mark project as completed. {incompletePhasesCount} phases are not yet completed.");

            // Update project status - Should this be Completion_Approved instead?
            project.Status = (int)ProjectStatusEnum.Completed; // Use the new status
            project.UpdatedAt = DateTime.UtcNow;
            // Potentially store the user who approved it in the ProjectRequests table

            await _projectRepository.UpdateAsync(project);

            // Update the corresponding ProjectRequest status
             var requestToUpdate = await _context.ProjectRequests
                 .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.RequestType == ProjectRequestTypeEnum.Completion && r.ApprovalStatus == ApprovalStatusEnum.Pending);

             if (requestToUpdate != null)
             {
                 requestToUpdate.ApprovalStatus = ApprovalStatusEnum.Approved;
                 requestToUpdate.ApprovedById = userId; // Assuming the user calling this IS the approver
                 requestToUpdate.ApprovedAt = DateTime.UtcNow;
                 _context.ProjectRequests.Update(requestToUpdate);
                 await _context.SaveChangesAsync();
             }


            // Send notifications to group members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Completion Approved", // Update title
                    Message = $"Project '{project.ProjectName}' completion request has been approved.", // Update message
                    ProjectId = project.ProjectId,
                    Status = 0, // Or appropriate status
                    IsRead = false
                };

                await _notificationService.CreateNotification(notificationRequest);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error marking project as completed: {ex.Message}");
        }
    }

    public async Task RequestProjectCompletionAsync(int projectId, int userId, RequestProjectCompletionRequest request, IEnumerable<IFormFile>? finalDocuments)
    {
        // Use a transaction to ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // --- Validation ---
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .Include(p => p.Group) // Include group to check user membership
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException("Project not found");

            // Check user permissions (must be a member of the project group)
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == project.GroupId && gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active);

            if (!isMember)
                throw new ServiceException("You are not authorized to request completion for this project.");

            // Check project status (must be Approved to request completion)
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException($"Project must be in '{ProjectStatusEnum.Approved}' status to request completion. Current status: {(ProjectStatusEnum?)project.Status}");

            // Verify all phases are completed
            if (!project.ProjectPhases.Any())
                throw new ServiceException("Project has no phases defined.");

            var incompletePhasesCount = project.ProjectPhases
                .Count(p => p.Status != (int)ProjectPhaseStatusEnum.Completed);

            if (incompletePhasesCount > 0)
                throw new ServiceException($"Cannot request completion. {incompletePhasesCount} project phases are not yet marked as completed.");

            // Verify budget reconciliation confirmation
            if (!request.BudgetReconciled)
                throw new ServiceException("Budget must be confirmed as reconciled before submitting completion request.");

            // --- Create Request Records ---

            // 1. Create the main ProjectRequest entry
            var projectRequest = new ProjectRequest
            {
                ProjectId = projectId,
                RequestType = ProjectRequestTypeEnum.Completion,
                RequestedById = userId,
                RequestedAt = DateTime.UtcNow, // Use UTC for consistency
                ApprovalStatus = ApprovalStatusEnum.Pending,
                // TODO: Determine how to assign council/timeline if needed for completion review
                // AssignedCouncilId = DetermineCouncilGroup(project.DepartmentId),
                // TimelineId = FindActiveReviewTimeline(),
            };
            await _context.ProjectRequests.AddAsync(projectRequest);
            await _context.SaveChangesAsync(); // Save to get the request_id

            // Calculate the remaining budget from project data
            decimal calculatedBudgetRemaining = (project.ApprovedBudget ?? 0) - project.SpentBudget;

            // Create CompletionRequestDetail entry with the calculated budget
            var completionDetail = new CompletionRequestDetail
            {
                RequestId = projectRequest.RequestId,
                CompletionSummary = request.CompletionSummary,
                BudgetReconciled = request.BudgetReconciled,
                BudgetVarianceExplanation = request.BudgetVarianceExplanation,
                BudgetRemaining = calculatedBudgetRemaining // Use server-calculated value
            };
            await _context.CompletionRequestDetails.AddAsync(completionDetail);

            // --- Handle Document Uploads ---
            if (finalDocuments != null && finalDocuments.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(finalDocuments, $"projects/{projectId}/completion-documents");
                int index = 0;
                foreach (var file in finalDocuments)
                {
                    // Create ProjectResource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document type
                        ProjectId = projectId,
                        Acquired = true, // Assuming acquired upon upload
                        Quantity = 1
                    };
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync(); // Save to get ResourceId

                    // Create Document linked to the resource and project
                    var document = new Document
                    {
                        ProjectId = projectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        // TODO: Add a specific DocumentTypeEnum value for final reports, e.g., FinalReport
                        DocumentType = (int)DocumentTypeEnum.ProjectProposal, // Replace with actual final report type
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId
                    };
                    await _context.Documents.AddAsync(document);
                    index++;
                }
            }

            // --- Update Project Status ---
            project.Status = (int)ProjectStatusEnum.Completion_Requested;
            project.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(project);

            // --- Save All Changes ---
            await _context.SaveChangesAsync();

            // --- Notifications ---
            // TODO: Implement notification logic
            // Find relevant council members/secretary based on department or assigned council
            // var councilMembers = await FindCouncilMembersToNotify(project.DepartmentId, projectRequest.AssignedCouncilId);
            // foreach (var member in councilMembers)
            // {
            //     await _notificationService.CreateNotification(new CreateNotificationRequest { ... });
            // }

            // --- Commit Transaction ---
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the exception details (using a proper logging framework is recommended)
            Console.WriteLine($"Error requesting project completion for project {projectId}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            // Re-throw as ServiceException or a more specific exception type
            throw new ServiceException($"An error occurred while submitting the completion request: {ex.Message}", ex);
        }
    }

    public async Task AddCompletionDocumentsAsync(int projectId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            // Verification code remains the same
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");
            
            if (project.Status != (int)ProjectStatusEnum.Completion_Requested && 
                project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Documents can only be added to projects with a pending completion request or approved status");
            
            // Permission check remains the same
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == project.GroupId && gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active);
            
            if (!isMember)
                throw new ServiceException("You are not authorized to add completion documents to this project");
            
            // Find the associated completion request
            var completionRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && 
                                    r.RequestType == ProjectRequestTypeEnum.Completion &&
                                    r.ApprovalStatus != ApprovalStatusEnum.Rejected);
            
            if (completionRequest == null)
                throw new ServiceException("No active completion request found for this project");
            
            // File upload logic
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/completion-documents");
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document type
                        ProjectId = projectId,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    var document = new Document
                    {
                        ProjectId = projectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.ProjectCompletion, // Use the new document type
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        RequestId = completionRequest.RequestId // Link to the request
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding completion documents: {ex.Message}");
            throw new ServiceException($"Error adding completion documents: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<CompletionRequestResponse>> GetCompletionRequestsAsync()
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.CompletionRequestDetail) // Add this to include completion details
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return requests.Select(r => {
                var response = new CompletionRequestResponse
                {
                    // Request information
                    RequestId = r.RequestId,
                    RequestType = r.RequestType,
                    ApprovalStatus = r.ApprovalStatus,
                    RequestedAt = r.RequestedAt,
                    RejectionReason = r.RejectionReason,
                    
                    // Requester information
                    RequestedById = r.RequestedById,
                    RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                    
                    // Approver information
                    ApprovedById = r.ApprovedById,
                    ApproverName = r.ApprovedBy?.FullName,
                    ApprovedAt = r.ApprovedAt,
                    
                    // Project information
                    ProjectId = r.ProjectId,
                    ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                    ProjectDescription = r.Project?.Description ?? "",
                    ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                    ApprovedBudget = r.Project?.ApprovedBudget,
                    SpentBudget = r.Project?.SpentBudget ?? 0,
                    
                    // Department information
                    DepartmentId = r.Project?.DepartmentId,
                    DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown Department",
                    
                    // Group information
                    GroupId = r.Project?.GroupId,
                    GroupName = r.Project?.Group?.GroupName ?? "Unknown Group",
                    
                    // Default completion values
                    BudgetRemaining = null,
                    BudgetReconciled = false,
                    CompletionSummary = null,
                    BudgetVarianceExplanation = null
                };
                
                // Add completion-specific details if available
                if (r.CompletionRequestDetail != null)
                {
                    response.BudgetRemaining = r.CompletionRequestDetail.BudgetRemaining;
                    response.BudgetReconciled = r.CompletionRequestDetail.BudgetReconciled;
                    response.CompletionSummary = r.CompletionRequestDetail.CompletionSummary;
                    response.BudgetVarianceExplanation = r.CompletionRequestDetail.BudgetVarianceExplanation;
                }
                
                return response;
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting project requests: {ex.Message}");
            throw new ServiceException($"Error retrieving project requests: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApproveCompletionRequestAsync(int requestId, int approverId, IEnumerable<IFormFile> documents)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Verify request exists and is in pending state
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                    r.RequestType == ProjectRequestTypeEnum.Completion);
            
            if (request == null)
                throw new ServiceException("Completion request not found");
            
            if (request.ApprovalStatus != ApprovalStatusEnum.Pending)
                throw new ServiceException($"Cannot approve request with status: {request.ApprovalStatus}");
            
            // 2. Verify project is in Completion_Requested status
            var project = request.Project;
            if (project.Status != (int)ProjectStatusEnum.Completion_Requested)
                throw new ServiceException("Project is not in a completion requested state");
            
            // 3. Verify approver has permission (council member)
            var approverGroups = await _groupRepository.GetGroupsByUserId(approverId);
            bool isCouncilMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == approverId && 
                         gm.Group.GroupType == (int)GroupTypeEnum.Council &&
                         gm.Status == (int)GroupMemberStatus.Active);
                         
            if (!isCouncilMember)
                throw new ServiceException("Only council members can approve completion requests");
            
            // 4. Process document uploads if any
            if (documents != null && documents.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documents, $"projects/{project.ProjectId}/completion-approval");
                int index = 0;
                
                foreach (var file in documents)
                {
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document type
                        ProjectId = project.ProjectId,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    var document = new Document
                    {
                        ProjectId = project.ProjectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = approverId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        RequestId = requestId // Link to the request
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            // 5. Update request status
            request.ApprovalStatus = ApprovalStatusEnum.Approved;
            request.ApprovedById = approverId;
            request.ApprovedAt = DateTime.UtcNow;
            
            // 6. Update project status to Completed instead of Completion_Approved
            project.Status = (int)ProjectStatusEnum.Completed;
            project.UpdatedAt = DateTime.UtcNow;
            
            _context.ProjectRequests.Update(request);
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            
            // 7. Send notifications
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Completion Approved",
                    Message = $"Project '{project.ProjectName}' completion request has been approved.",
                    ProjectId = project.ProjectId
                };
                
                await _notificationService.CreateNotification(notificationRequest);
            }
            
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error approving completion request: {ex.Message}", ex);
        }
    }

    public async Task<bool> RejectCompletionRequestAsync(int requestId, int rejecterId, string rejectionReason, IEnumerable<IFormFile> documents)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Similar validations as approve method
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                    r.RequestType == ProjectRequestTypeEnum.Completion);
            
            if (request == null)
                throw new ServiceException("Completion request not found");
            
            if (request.ApprovalStatus != ApprovalStatusEnum.Pending)
                throw new ServiceException($"Cannot reject request with status: {request.ApprovalStatus}");
            
            var project = request.Project;
            if (project.Status != (int)ProjectStatusEnum.Completion_Requested)
                throw new ServiceException("Project is not in a completion requested state");
            
            // Verify rejecter has permission
            var rejecterGroups = await _groupRepository.GetGroupsByUserId(rejecterId);
            bool isCouncilMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == rejecterId && 
                         gm.Group.GroupType == (int)GroupTypeEnum.Council &&
                         gm.Status == (int)GroupMemberStatus.Active);
                         
            if (!isCouncilMember)
                throw new ServiceException("Only council members can reject completion requests");
            
            // Require rejection reason
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new ServiceException("Rejection reason is required");
            
            // Process documents
            if (documents != null && documents.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documents, $"projects/{project.ProjectId}/completion-rejection");
                int index = 0;
                
                foreach (var file in documents)
                {
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document type
                        ProjectId = project.ProjectId,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    var document = new Document
                    {
                        ProjectId = project.ProjectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = rejecterId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        RequestId = requestId // Link to the request
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            // Update request status
            request.ApprovalStatus = ApprovalStatusEnum.Rejected;
            request.ApprovedById = rejecterId; // Using same field even for rejection
            request.ApprovedAt = DateTime.UtcNow;
            request.RejectionReason = rejectionReason;
            
            // Set status back to Approved
            project.Status = (int)ProjectStatusEnum.Approved;
            // Record rejection reason
            project.RejectionReason = rejectionReason;
            project.UpdatedAt = DateTime.UtcNow;
            
            _context.ProjectRequests.Update(request);
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            
            // Send notifications
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            foreach (var member in groupMembers)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.UserId.Value,
                    Title = "Project Completion Rejected",
                    Message = $"Project '{project.ProjectName}' completion request has been rejected. Reason: {rejectionReason}",
                    ProjectId = project.ProjectId
                };
                
                await _notificationService.CreateNotification(notificationRequest);
            }
            
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error rejecting completion request: {ex.Message}", ex);
        }
    }

    public async Task<CompletionRequestDetailResponse> GetCompletionRequestByIdAsync(int requestId)
    {
        try
        {
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.CompletionRequestDetail)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                    r.RequestType == ProjectRequestTypeEnum.Completion);
            
            if (request == null)
                throw new ServiceException("Completion request not found");
            
            // Fetch documents related to this request
            var documents = await _context.Documents
                .Where(d => d.RequestId == requestId)
                .ToListAsync();
            
            return new CompletionRequestDetailResponse
            {
                // Basic info from the request
                RequestId = request.RequestId,
                ProjectId = request.ProjectId,
                ProjectName = request.Project?.ProjectName ?? "Unknown Project",
                ApprovalStatus = request.ApprovalStatus,
                RequestedAt = request.RequestedAt,
                RequestedById = request.RequestedById,
                RequesterName = request.RequestedBy?.FullName ?? "Unknown",
                ApprovedAt = request.ApprovedAt,
                ApproverName = request.ApprovedBy?.FullName ?? "",
                
                // CompletionRequestDetail data
                BudgetRemaining = request.CompletionRequestDetail?.BudgetRemaining,
                BudgetReconciled = request.CompletionRequestDetail?.BudgetReconciled ?? false,
                CompletionSummary = request.CompletionRequestDetail?.CompletionSummary,
                BudgetVarianceExplanation = request.CompletionRequestDetail?.BudgetVarianceExplanation,
                
                // Additional project data
                ApprovedBudget = request.Project?.ApprovedBudget,
                SpentBudget = request.Project?.SpentBudget ?? 0,
                ProjectStatus = (ProjectStatusEnum)(request.Project?.Status ?? 0),
                ProjectDescription = request.Project?.Description,
                ProjectStartDate = request.Project?.StartDate,
                ProjectEndDate = request.Project?.EndDate,
                
                // Documents related to this request
                Documents = _mapper.Map<ICollection<DocumentResponse>>(documents)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting completion request details: {ex.Message}");
            throw new ServiceException($"Error retrieving completion request details: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<CompletionRequestResponse>> GetUserCompletionRequestsAsync(int userId)
    {
        try
        {
            var userRequests = await _context.ProjectRequests
                .Include(r => r.Project)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.CompletionRequestDetail)
                .Where(r => r.RequestType == ProjectRequestTypeEnum.Completion && 
                           r.RequestedById == userId)
                .ToListAsync();
            
            return userRequests.Select(r => new CompletionRequestResponse
            {
                RequestId = r.RequestId,
                ProjectId = r.ProjectId,
                ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                ApprovalStatus = r.ApprovalStatus,
                RequestedAt = r.RequestedAt,
                RequestedById = r.RequestedById,
                RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                ApprovedAt = r.ApprovedAt,
                ApproverName = r.ApprovedBy?.FullName ?? "",
                
                // CompletionRequestDetail data
                BudgetRemaining = r.CompletionRequestDetail?.BudgetRemaining,
                BudgetReconciled = r.CompletionRequestDetail?.BudgetReconciled ?? false,
                CompletionSummary = r.CompletionRequestDetail?.CompletionSummary,
                BudgetVarianceExplanation = r.CompletionRequestDetail?.BudgetVarianceExplanation,
                
                // Additional project data
                ApprovedBudget = r.Project?.ApprovedBudget,
                SpentBudget = r.Project?.SpentBudget ?? 0
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user completion requests: {ex.Message}");
            throw new ServiceException($"Error retrieving user's completion requests: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApproveProjectRequestAsync(int requestId, int secretaryId, IEnumerable<IFormFile> documentFiles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
            
            if (request == null)
                throw new ServiceException("Project request not found");
            
            // Check existing logic for approving different request types...
            
            // Add this new section for Fund_Disbursement request type
            if (request.RequestType == ProjectRequestTypeEnum.Fund_Disbursement)
            {
                if (!request.FundDisbursementId.HasValue)
                    throw new ServiceException("Fund disbursement request is missing a reference to the fund disbursement");
                
                // Get the fund disbursement
                var fundDisbursement = await _context.FundDisbursements
                    .Include(fd => fd.Project)
                    .Include(fd => fd.Quota)
                    .FirstOrDefaultAsync(fd => fd.FundDisbursementId == request.FundDisbursementId.Value);
                
                if (fundDisbursement == null)
                    throw new ServiceException("Associated fund disbursement not found");
                
                // Check if fund disbursement is already processed
                if (fundDisbursement.Status != (int)FundDisbursementStatusEnum.Pending)
                    throw new ServiceException($"This fund disbursement request has already been {(fundDisbursement.Status == (int)FundDisbursementStatusEnum.Approved ? "approved" : "rejected")}");
                
                // Find or create a group member for the secretary
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.UserId == secretaryId);
                
                if (groupMember == null)
                {
                    // Create a temporary group member entry for the secretary
                    var defaultGroup = await _context.Groups.FirstOrDefaultAsync();
                    if (defaultGroup == null)
                        throw new ServiceException("No groups available to associate with approver");
                    
                    groupMember = new GroupMember
                    {
                        UserId = secretaryId,
                        GroupId = defaultGroup.GroupId,
                        Role = (int)GroupMemberRoleEnum.Member,
                        Status = (int)GroupMemberStatus.Active,
                        JoinDate = DateTime.Now
                    };
                    
                    _context.GroupMembers.Add(groupMember);
                    await _context.SaveChangesAsync();
                }
                
                // Update the fund disbursement
                fundDisbursement.Status = (int)FundDisbursementStatusEnum.Approved;
                fundDisbursement.AppovedBy = groupMember.GroupMemberId;
                fundDisbursement.UpdateAt = DateTime.Now;
                
                // Update the quota's remaining budget
                var quota = fundDisbursement.Quota;
                if (quota == null)
                    throw new ServiceException("Quota not found for this fund disbursement");
                
                quota.AllocatedBudget -= fundDisbursement.FundRequest ?? 0;
                quota.UpdateAt = DateTime.Now;
                
                // If quota is fully used, update its status
                if (quota.AllocatedBudget <= 0)
                {
                    quota.Status = (int)QuotaStatusEnum.Used;
                }
                
                // Update the project's spent budget
                var project = fundDisbursement.Project;
                if (project == null)
                    throw new ServiceException("Project not found for this fund disbursement");
                
                // Check if approving this disbursement would violate the budget constraint
                // if (project.SpentBudget + (fundDisbursement.FundRequest ?? 0) > project.ApprovedBudget)
                //     throw new ServiceException($"Approving this disbursement would exceed the project's approved budget. Project approved budget: {project.ApprovedBudget}, current spent: {project.SpentBudget}, requested amount: {fundDisbursement.FundRequest}");
                
                project.SpentBudget += fundDisbursement.FundRequest ?? 0;
                project.UpdatedAt = DateTime.Now;

                // Update project phase if applicable
                if (fundDisbursement.ProjectPhaseId.HasValue)
                {
                    var projectPhase = await _context.ProjectPhases.FindAsync(fundDisbursement.ProjectPhaseId.Value);
                    if (projectPhase != null)
                    {
                        projectPhase.SpentBudget += fundDisbursement.FundRequest ?? 0;
                        _context.ProjectPhases.Update(projectPhase);
                    }
                }
                
                _context.FundDisbursements.Update(fundDisbursement);
            }
            
            // Add handling for Research_Creation
            if (request.RequestType == ProjectRequestTypeEnum.Research_Creation)
            {
                var project = request.Project;
                if (project == null)
                    throw new ServiceException("Project not found");
                
                // Update project status to Approved
                project.Status = (int)ProjectStatusEnum.Approved;
                project.UpdatedAt = DateTime.Now;
                _context.Projects.Update(project);
                
                // Create a new quota with the same budget as the project
                // First check if a quota already exists for this project
                var existingQuota = await _context.Quotas
                    .FirstOrDefaultAsync(q => q.ProjectId == project.ProjectId);
                
                if (existingQuota == null)
                {
                    var quota = new Quota
                    {
                        AllocatedBudget = project.ApprovedBudget,
                        Status = (int)QuotaStatusEnum.Active,
                        CreatedAt = DateTime.Now,
                        ProjectId = project.ProjectId,
                        AllocatedBy = secretaryId
                    };

                    await _context.Quotas.AddAsync(quota);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Created quota {quota.QuotaId} for project {project.ProjectId} with budget {quota.AllocatedBudget}");
                }
                else
                {
                    Console.WriteLine($"Quota {existingQuota.QuotaId} already exists for project {project.ProjectId}");
                }
                
                // Send notifications to research group members if the group exists
                if (project.GroupId.HasValue)
                {
                    var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
                    foreach (var member in groupMembers)
                    {
                        if (member.UserId.HasValue)
                        {
                            var notificationRequest = new CreateNotificationRequest
                            {
                                UserId = member.UserId.Value,
                                Title = "Project Approved",
                                Message = $"Project '{project.ProjectName}' has been approved",
                                ProjectId = project.ProjectId
                            };
                            await _notificationService.CreateNotification(notificationRequest);
                        }
                    }
                }
            }
            
            // Common request update code
            request.ApprovalStatus = ApprovalStatusEnum.Approved;
            request.ApprovedById = secretaryId;
            request.ApprovedAt = DateTime.Now;
            
            _context.ProjectRequests.Update(request);
            await _context.SaveChangesAsync();
            
            // Process documents if any
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{request.ProjectId}/request-approval");
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create a ProjectResource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName ?? "Approval Document",
                        ResourceType = 1, // Document type
                        Cost = 0,
                        Quantity = 1,
                        Acquired = true,
                        ProjectId = request.ProjectId
                    };
                    
                    _context.ProjectResources.Add(projectResource);
                    await _context.SaveChangesAsync(); // Save to get the ID
                    
                    // Create the Document with the ProjectResourceId
                    var document = new Document
                    {
                        ProjectId = request.ProjectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                        UploadAt = DateTime.Now,
                        UploadedBy = secretaryId,
                        RequestId = requestId,
                        ProjectResourceId = projectResource.ProjectResourceId
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error approving project request: {ex.Message}");
        }
    }

    public async Task<bool> RejectProjectRequestAsync(int requestId, int secretaryId, string rejectionReason, IEnumerable<IFormFile> documentFiles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
            
            if (request == null)
                throw new ServiceException("Project request not found");
            
            // Add this section for Fund_Disbursement request type
            if (request.RequestType == ProjectRequestTypeEnum.Fund_Disbursement)
            {
                if (!request.FundDisbursementId.HasValue)
                    throw new ServiceException("Fund disbursement request is missing a reference to the fund disbursement");
                
                var fundDisbursement = await _context.FundDisbursements
                    .FirstOrDefaultAsync(fd => fd.FundDisbursementId == request.FundDisbursementId.Value);
                
                if (fundDisbursement == null)
                    throw new ServiceException("Associated fund disbursement not found");
                
                if (fundDisbursement.Status != (int)FundDisbursementStatusEnum.Pending)
                    throw new ServiceException($"This fund disbursement request has already been {(fundDisbursement.Status == (int)FundDisbursementStatusEnum.Approved ? "approved" : "rejected")}");
                
                // Find or create a group member for the secretary
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.UserId == secretaryId);
                
                if (groupMember == null)
                {
                    // Create a temporary group member entry for the secretary
                    var defaultGroup = await _context.Groups.FirstOrDefaultAsync();
                    if (defaultGroup == null)
                        throw new ServiceException("No groups available to associate with rejector");
                    
                    groupMember = new GroupMember
                    {
                        UserId = secretaryId,
                        GroupId = defaultGroup.GroupId,
                        Role = (int)GroupMemberRoleEnum.Member,
                        Status = (int)GroupMemberStatus.Active,
                        JoinDate = DateTime.Now
                    };
                    
                    _context.GroupMembers.Add(groupMember);
                    await _context.SaveChangesAsync();
                }
                
                // Update fund disbursement
                fundDisbursement.Status = (int)FundDisbursementStatusEnum.Rejected;
                fundDisbursement.AppovedBy = groupMember.GroupMemberId; // Used for both approval and rejection
                fundDisbursement.RejectionReason = rejectionReason;
                fundDisbursement.UpdateAt = DateTime.Now;
                
                _context.FundDisbursements.Update(fundDisbursement);
                
                // Send notification to the requester that they can submit a new request
                if (fundDisbursement.UserRequest.HasValue)
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = fundDisbursement.UserRequest.Value,
                        Title = "Fund Disbursement Request Rejected",
                        Message = $"Your fund disbursement request has been rejected. Reason: {rejectionReason}. You can submit a new request if needed.",
                        ProjectId = request.ProjectId,
                        Status = 0,
                        IsRead = false
                    };
                    
                    await _notificationService.CreateNotification(notificationRequest);
                }
            }
            
            // Add handling for Research_Creation
            if (request.RequestType == ProjectRequestTypeEnum.Research_Creation)
            {
                var project = request.Project;
                if (project == null)
                    throw new ServiceException("Project not found");
                
                // Update project status to Rejected
                project.Status = (int)ProjectStatusEnum.Rejected;
                project.UpdatedAt = DateTime.Now;
                project.RejectionReason = rejectionReason;
                _context.Projects.Update(project);
            }
            
            // Common request update
            request.ApprovalStatus = ApprovalStatusEnum.Rejected;
            request.ApprovedById = secretaryId;
            request.ApprovedAt = DateTime.Now;
            request.RejectionReason = rejectionReason;
            
            _context.ProjectRequests.Update(request);
            await _context.SaveChangesAsync();
            
            // Process documents if any
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{request.ProjectId}/request-rejection");
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create a ProjectResource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName ?? "Rejection Document",
                        ResourceType = 1, // Document type
                        Cost = 0,
                        Quantity = 1,
                        Acquired = true,
                        ProjectId = request.ProjectId
                    };
                    
                    _context.ProjectResources.Add(projectResource);
                    await _context.SaveChangesAsync(); // Save to get the ID
                    
                    // Create the Document with the ProjectResourceId
                    var document = new Document
                    {
                        ProjectId = request.ProjectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                        UploadAt = DateTime.Now,
                        UploadedBy = secretaryId,
                        RequestId = requestId,
                        ProjectResourceId = projectResource.ProjectResourceId 
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error rejecting project request: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectRequestResponse>> GetAllProjectRequestsAsync()
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Include(r => r.CompletionRequestDetail) // Add this line
                .ToListAsync();

            return requests.Select(r => {
                var response = new ProjectRequestResponse
                {
                    RequestId = r.RequestId,
                    ProjectId = r.ProjectId,
                    RequestType = r.RequestType,
                    RequestedById = r.RequestedById,
                    RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                    ApprovalStatus = r.ApprovalStatus,
                    StatusName = r.ApprovalStatus.HasValue ? 
                        Enum.GetName(typeof(ApprovalStatusEnum), r.ApprovalStatus.Value) : null,
                    RequestedAt = r.RequestedAt,
                    ApprovedById = r.ApprovedById,
                    ApproverName = r.ApprovedBy?.FullName,
                    ApprovedAt = r.ApprovedAt,
                    RejectionReason = r.RejectionReason,
                    ProjectName = r.Project?.ProjectName ?? "Unknown",
                    ProjectDescription = r.Project?.Description ?? "",
                    ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                    ApprovedBudget = r.Project?.ApprovedBudget,
                    SpentBudget = r.Project?.SpentBudget ?? 0,
                    DepartmentId = r.Project?.DepartmentId,
                    DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown",
                    GroupId = r.Project?.GroupId,
                    GroupName = r.Project?.Group?.GroupName ?? "Unknown",
                    ProjectType = r.Project?.ProjectType,
                    ProjectTypeName = r.Project?.ProjectType.HasValue == true ? 
                        Enum.GetName(typeof(ProjectTypeEnum), r.Project.ProjectType.Value) : null,
                    FundDisbursementId = r.FundDisbursementId,
                    FundRequestAmount = r.FundDisbursement?.FundRequest,
                    
                    // Set default completion values
                    BudgetRemaining = null,
                    BudgetReconciled = null,
                    CompletionSummary = null,
                    BudgetVarianceExplanation = null
                };
                
                // Add completion-specific details if available
                if (r.RequestType == ProjectRequestTypeEnum.Completion && r.CompletionRequestDetail != null)
                {
                    response.BudgetRemaining = r.CompletionRequestDetail.BudgetRemaining;
                    response.BudgetReconciled = r.CompletionRequestDetail.BudgetReconciled;
                    response.CompletionSummary = r.CompletionRequestDetail.CompletionSummary;
                    response.BudgetVarianceExplanation = r.CompletionRequestDetail.BudgetVarianceExplanation;
                }
                
                return response;
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving project requests: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectRequestResponse>> GetDepartmentProjectRequestsAsync(int departmentId)
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Where(r => r.Project.DepartmentId == departmentId)
                .ToListAsync();

            return requests.Select(r => new ProjectRequestResponse
            {
                // Request information
                RequestId = r.RequestId,
                RequestType = r.RequestType,
                ApprovalStatus = r.ApprovalStatus,
                RequestedAt = r.RequestedAt,
                RejectionReason = r.RejectionReason,
                
                // Requester information
                RequestedById = r.RequestedById,
                RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                
                // Approver information
                ApprovedById = r.ApprovedById,
                ApproverName = r.ApprovedBy?.FullName,
                ApprovedAt = r.ApprovedAt,
                
                // Project information
                ProjectId = r.ProjectId,
                ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                ProjectDescription = r.Project?.Description ?? "",
                ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                ApprovedBudget = r.Project?.ApprovedBudget,
                SpentBudget = r.Project?.SpentBudget ?? 0,
                
                // Department information
                DepartmentId = r.Project?.DepartmentId,
                DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown Department",
                
                // Group information
                GroupId = r.Project?.GroupId,
                GroupName = r.Project?.Group?.GroupName ?? "Unknown Group",
                
                // Status name
                StatusName = ((ApprovalStatusEnum?)r.ApprovalStatus)?.ToString() ?? "Unknown",
                ProjectType = r.Project?.ProjectType,
                ProjectTypeName = r.Project?.ProjectType.HasValue == true ? 
                    Enum.GetName(typeof(ProjectTypeEnum), r.Project.ProjectType.Value) : null,
                FundDisbursementId = r.FundDisbursementId,
                FundRequestAmount = r.FundDisbursement?.FundRequest
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving department project requests: {ex.Message}");
        }
    }

    public async Task<bool> AssignTimelineToRequestAsync(int requestId, int timelineId)
    {
        var request = await _context.ProjectRequests.FindAsync(requestId);
        if (request == null)
            throw new ServiceException("Request not found");
        
    var timeline = await _context.Timelines.FindAsync(timelineId);
    if (timeline == null)
        throw new ServiceException("Timeline not found");
    
    request.TimelineId = timelineId;
    await _context.SaveChangesAsync();
    return true;
    }

    public async Task<int> AssignTimelineToDepartmentRequestsAsync(int departmentId, int timelineId, ProjectRequestTypeEnum? requestType = null)
    {
        var timeline = await _context.Timelines.FindAsync(timelineId);
        if (timeline == null)
            throw new ServiceException("Timeline not found");
        
        // Find all requests for projects in the specified department
        var query = _context.ProjectRequests
            .Include(r => r.Project)
            .Where(r => r.Project.DepartmentId == departmentId && r.ApprovalStatus == ApprovalStatusEnum.Pending);
        
        // Optionally filter by request type if specified
        if (requestType.HasValue)
            query = query.Where(r => r.RequestType == requestType.Value);
        
        var requests = await query.ToListAsync();
        
        if (!requests.Any())
            throw new ServiceException("No pending project requests found for this department");
        
        // Assign the timeline to all matching requests
        foreach (var request in requests)
        {
            request.TimelineId = timelineId;
        }
        
        await _context.SaveChangesAsync();
        return requests.Count; // Return the number of affected requests
    }

    public async Task<ProjectRequestDetailResponse> GetProjectRequestDetailsAsync(int requestId)
    {
        try
        {
            var request = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                            .ThenInclude(gm => gm.User)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Documents)
                .Include(r => r.Project)
                    .ThenInclude(p => p.ProjectPhases)
                .Include(r => r.Project)
                    .ThenInclude(p => p.CreatedByNavigation)
                .Include(r => r.Project)
                    .ThenInclude(p => p.ApprovedByNavigation)
                        .ThenInclude(gm => gm.User)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Include(r => r.CompletionRequestDetail) // Add this line
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                throw new ServiceException("Project request not found");

            var project = request.Project;
            if (project == null)
                throw new ServiceException("Project associated with this request not found");

            var detailResponse = new ProjectRequestDetailResponse
            {
                // Request information
                RequestId = request.RequestId,
                RequestType = request.RequestType,
                ApprovalStatus = request.ApprovalStatus,
                StatusName = request.ApprovalStatus.HasValue ? 
                    Enum.GetName(typeof(ApprovalStatusEnum), request.ApprovalStatus.Value) : null,
                RequestedAt = request.RequestedAt,
                RejectionReason = request.RejectionReason,
                
                // Requester information
                RequestedBy = request.RequestedBy != null ? new UserShortInfo
                {
                    UserId = request.RequestedBy.UserId,
                    Username = request.RequestedBy.Username,
                    FullName = request.RequestedBy.FullName,
                    Email = request.RequestedBy.Email
                } : null,
                
                // Approver information
                ApprovedBy = request.ApprovedBy != null ? new UserShortInfo
                {
                    UserId = request.ApprovedBy.UserId,
                    Username = request.ApprovedBy.Username,
                    FullName = request.ApprovedBy.FullName,
                    Email = request.ApprovedBy.Email
                } : null,
                ApprovedAt = request.ApprovedAt,
                
                // Fund disbursement information
                FundDisbursementId = request.FundDisbursementId,
                FundRequestAmount = request.FundDisbursement?.FundRequest,
                
                // Project information
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                ProjectType = project.ProjectType,
                ProjectTypeName = project.ProjectType.HasValue ? 
                    Enum.GetName(typeof(ProjectTypeEnum), project.ProjectType.Value) : null,
                Description = project.Description,
                ApprovedBudget = project.ApprovedBudget,
                SpentBudget = project.SpentBudget,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Methodology = project.Methodlogy,
                
                // Group information
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
                
                // Department information
                Department = project.Department != null ? new DepartmentResponse
                {
                    DepartmentId = project.Department.DepartmentId,
                    DepartmentName = project.Department.DepartmentName
                } : null,
                
                // Project phases and documents
                ProjectPhases = _mapper.Map<ICollection<ProjectPhaseResponse>>(project.ProjectPhases),
                Documents = _mapper.Map<ICollection<DocumentResponse>>(project.Documents),
                
                // Completion-specific fields (with default null values)
                BudgetRemaining = null,
                BudgetReconciled = null,
                CompletionSummary = null,
                BudgetVarianceExplanation = null
            };
            
            // Add completion details if this is a completion request
            if (request.RequestType == ProjectRequestTypeEnum.Completion && request.CompletionRequestDetail != null)
            {
                detailResponse.BudgetRemaining = request.CompletionRequestDetail.BudgetRemaining;
                detailResponse.BudgetReconciled = request.CompletionRequestDetail.BudgetReconciled;
                detailResponse.CompletionSummary = request.CompletionRequestDetail.CompletionSummary;
                detailResponse.BudgetVarianceExplanation = request.CompletionRequestDetail.BudgetVarianceExplanation;
            }
            
            return detailResponse;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving project request details: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectRequestResponse>> GetPendingDepartmentRequestsAsync(int departmentId)
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Include(r => r.CompletionRequestDetail) // Add this line
                .Where(r => r.Project.DepartmentId == departmentId && 
                            r.ApprovalStatus == ApprovalStatusEnum.Pending &&
                            r.RequestType != ProjectRequestTypeEnum.Fund_Disbursement)
                .ToListAsync();

            return requests.Select(r => {
                var response = new ProjectRequestResponse
                {
                    // Map all existing properties
                    RequestId = r.RequestId,
                    RequestType = r.RequestType,
                    ApprovalStatus = r.ApprovalStatus,
                    RequestedAt = r.RequestedAt,
                    RejectionReason = r.RejectionReason,
                    RequestedById = r.RequestedById,
                    RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                    ApprovedById = r.ApprovedById,
                    ApproverName = r.ApprovedBy?.FullName,
                    ApprovedAt = r.ApprovedAt,
                    ProjectId = r.ProjectId,
                    ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                    ProjectDescription = r.Project?.Description ?? "",
                    ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                    ApprovedBudget = r.Project?.ApprovedBudget,
                    SpentBudget = r.Project?.SpentBudget ?? 0,
                    DepartmentId = r.Project?.DepartmentId,
                    DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown Department",
                    GroupId = r.Project?.GroupId,
                    GroupName = r.Project?.Group?.GroupName ?? "Unknown Group",
                    StatusName = ((ApprovalStatusEnum?)r.ApprovalStatus)?.ToString() ?? "Unknown",
                    ProjectType = r.Project?.ProjectType,
                    ProjectTypeName = r.Project?.ProjectType.HasValue == true ? 
                        Enum.GetName(typeof(ProjectTypeEnum), r.Project.ProjectType.Value) : null,
                    FundDisbursementId = null,
                    FundRequestAmount = null,
                    
                    // Set default completion values
                    BudgetRemaining = null,
                    BudgetReconciled = null,
                    CompletionSummary = null,
                    BudgetVarianceExplanation = null
                };
                
                // Add this section to populate completion data
                if (r.RequestType == ProjectRequestTypeEnum.Completion && r.CompletionRequestDetail != null)
                {
                    response.BudgetRemaining = r.CompletionRequestDetail.BudgetRemaining;
                    response.BudgetReconciled = r.CompletionRequestDetail.BudgetReconciled;
                    response.CompletionSummary = r.CompletionRequestDetail.CompletionSummary;
                    response.BudgetVarianceExplanation = r.CompletionRequestDetail.BudgetVarianceExplanation;
                }
                
                return response;
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving pending department project requests: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectRequestResponse>> GetUserProjectRequestsAsync(int userId)
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Include(r => r.CompletionRequestDetail) // Add this line
                .Where(r => r.RequestedById == userId)
                .ToListAsync();

            return requests.Select(r => {
                var response = new ProjectRequestResponse
                {
                    // Existing properties
                    RequestId = r.RequestId,
                    RequestType = r.RequestType,
                    ApprovalStatus = r.ApprovalStatus,
                    RequestedAt = r.RequestedAt,
                    RejectionReason = r.RejectionReason,
                    RequestedById = r.RequestedById,
                    RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                    ApprovedById = r.ApprovedById,
                    ApproverName = r.ApprovedBy?.FullName,
                    ApprovedAt = r.ApprovedAt,
                    ProjectId = r.ProjectId,
                    ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                    ProjectDescription = r.Project?.Description ?? "",
                    ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                    ApprovedBudget = r.Project?.ApprovedBudget,
                    SpentBudget = r.Project?.SpentBudget ?? 0,
                    DepartmentId = r.Project?.DepartmentId,
                    DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown Department",
                    GroupId = r.Project?.GroupId,
                    GroupName = r.Project?.Group?.GroupName ?? "Unknown Group",
                    StatusName = ((ApprovalStatusEnum?)r.ApprovalStatus)?.ToString() ?? "Unknown",
                    ProjectType = r.Project?.ProjectType,
                    ProjectTypeName = r.Project?.ProjectType.HasValue == true ? 
                        Enum.GetName(typeof(ProjectTypeEnum), r.Project.ProjectType.Value) : null,
                    FundDisbursementId = r.FundDisbursementId,
                    FundRequestAmount = r.FundDisbursement?.FundRequest,
                    
                    // Set default completion values
                    BudgetRemaining = null,
                    BudgetReconciled = null,
                    CompletionSummary = null,
                    BudgetVarianceExplanation = null
                };
                
                // Add completion-specific details if available
                if (r.RequestType == ProjectRequestTypeEnum.Completion && r.CompletionRequestDetail != null)
                {
                    response.BudgetRemaining = r.CompletionRequestDetail.BudgetRemaining;
                    response.BudgetReconciled = r.CompletionRequestDetail.BudgetReconciled;
                    response.CompletionSummary = r.CompletionRequestDetail.CompletionSummary;
                    response.BudgetVarianceExplanation = r.CompletionRequestDetail.BudgetVarianceExplanation;
                }
                
                return response;
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving user project requests: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProjectRequestResponse>> GetUserPendingProjectRequestsAsync(int userId)
    {
        try
        {
            var requests = await _context.ProjectRequests
                .Include(r => r.Project)
                    .ThenInclude(p => p.Department)
                .Include(r => r.Project)
                    .ThenInclude(p => p.Group)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.FundDisbursement)
                .Include(r => r.CompletionRequestDetail) // Add this line
                .Where(r => r.RequestedById == userId && 
                            r.ApprovalStatus == ApprovalStatusEnum.Pending)
                .ToListAsync();

            return requests.Select(r => {
                var response = new ProjectRequestResponse
                {
                    // Existing properties
                    RequestId = r.RequestId,
                    RequestType = r.RequestType,
                    ApprovalStatus = r.ApprovalStatus,
                    RequestedAt = r.RequestedAt,
                    RejectionReason = r.RejectionReason,
                    RequestedById = r.RequestedById,
                    RequesterName = r.RequestedBy?.FullName ?? "Unknown",
                    ApprovedById = r.ApprovedById,
                    ApproverName = r.ApprovedBy?.FullName,
                    ApprovedAt = r.ApprovedAt,
                    ProjectId = r.ProjectId,
                    ProjectName = r.Project?.ProjectName ?? "Unknown Project",
                    ProjectDescription = r.Project?.Description ?? "",
                    ProjectStatus = (ProjectStatusEnum)(r.Project?.Status ?? 0),
                    ApprovedBudget = r.Project?.ApprovedBudget,
                    SpentBudget = r.Project?.SpentBudget ?? 0,
                    DepartmentId = r.Project?.DepartmentId,
                    DepartmentName = r.Project?.Department?.DepartmentName ?? "Unknown Department",
                    GroupId = r.Project?.GroupId,
                    GroupName = r.Project?.Group?.GroupName ?? "Unknown Group",
                    StatusName = ((ApprovalStatusEnum?)r.ApprovalStatus)?.ToString() ?? "Unknown",
                    ProjectType = r.Project?.ProjectType,
                    ProjectTypeName = r.Project?.ProjectType.HasValue == true ? 
                        Enum.GetName(typeof(ProjectTypeEnum), r.Project.ProjectType.Value) : null,
                    FundDisbursementId = r.FundDisbursementId,
                    FundRequestAmount = r.FundDisbursement?.FundRequest,
                    
                    // Set default completion values
                    BudgetRemaining = null,
                    BudgetReconciled = null,
                    CompletionSummary = null,
                    BudgetVarianceExplanation = null
                };
                
                // Add completion-specific details if available
                if (r.RequestType == ProjectRequestTypeEnum.Completion && r.CompletionRequestDetail != null)
                {
                    response.BudgetRemaining = r.CompletionRequestDetail.BudgetRemaining;
                    response.BudgetReconciled = r.CompletionRequestDetail.BudgetReconciled;
                    response.CompletionSummary = r.CompletionRequestDetail.CompletionSummary;
                    response.BudgetVarianceExplanation = r.CompletionRequestDetail.BudgetVarianceExplanation;
                }
                
                return response;
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving user pending project requests: {ex.Message}");
        }
    }

    public async Task<ProjectCompletionSummaryResponse> GetProjectCompletionSummaryAsync(int projectId)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .Include(p => p.Documents)
                .Include(p => p.FundDisbursements)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupMembers)
                        .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException($"Project with ID {projectId} not found");

            var summary = new ProjectCompletionSummaryResponse
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                ProjectType = project.ProjectType,
                ProjectTypeName = project.ProjectType.HasValue ? 
                    Enum.GetName(typeof(ProjectTypeEnum), project.ProjectType.Value) : null,
                StartDate = project.StartDate ?? DateTime.MinValue,
                EndDate = project.EndDate ?? DateTime.MinValue,
                CreatedAt = project.CreatedAt ?? DateTime.MinValue,
                
                ApprovedBudget = project.ApprovedBudget ?? 0,
                SpentBudget = project.SpentBudget,
                
                TotalPhases = project.ProjectPhases.Count,
                CompletedPhases = project.ProjectPhases.Count(p => p.Status == (int)ProjectPhaseStatusEnum.Completed),
                Phases = _mapper.Map<ICollection<ProjectPhaseResponse>>(project.ProjectPhases),
                
                DocumentCount = project.Documents.Count,
                Documents = _mapper.Map<ICollection<DocumentResponse>>(project.Documents),
                
                TotalDisbursedAmount = project.FundDisbursements
                    .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                                fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                    .Sum(fd => fd.FundRequest ?? 0),
                DisbursementCount = project.FundDisbursements.Count,
                Disbursements = _mapper.Map<ICollection<FundDisbursementResponse>>(project.FundDisbursements),
                
                TeamMembers = project.Group?.GroupMembers
                    .Where(gm => gm.Status == (int)GroupMemberStatus.Active)
                    .Select(gm => new GroupMemberResponse
                    {
                        GroupMemberId = gm.GroupMemberId,
                        UserId = gm.UserId ?? 0,
                        MemberName = gm.User?.FullName ?? "Unknown",
                        MemberEmail = gm.User?.Email ?? "",
                        Role = gm.Role ?? 0,
                        JoinDate = gm.JoinDate ?? DateTime.MinValue,
                        Status = gm.Status ?? 0
                    }).ToList() ?? new List<GroupMemberResponse>()
            };

            return summary;
        }
        catch (Exception ex) when (ex is not ServiceException)
        {
            throw new ServiceException($"Error retrieving project completion summary: {ex.Message}");
        }
    }

}
