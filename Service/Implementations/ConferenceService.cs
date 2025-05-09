using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using Service.Settings;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Service.Implementations;

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IS3Service _s3Service;
    private readonly ITimelineService _timelineService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;
    private readonly IUserRepository _userRepository;

    public ConferenceService(
        IConferenceRepository conferenceRepository,
        IProjectRepository projectRepository,
        IGroupRepository groupRepository,
        INotificationService notificationService,
        IS3Service s3Service,
        ITimelineService timelineService,
        IMapper mapper,
        LRMSDbContext context,
        IEmailService emailService,
        IUserRepository userRepository)
    {
        _conferenceRepository = conferenceRepository;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
        _s3Service = s3Service;
        _timelineService = timelineService;
        _mapper = mapper;
        _context = context;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<ConferenceResponse> CreateConference(CreateConferenceRequest request, int createdBy)
    {
        try
        {
            // Kiểm tra project tồn tại
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");

            // Tạo conference
            var conference = new Conference
            {
                ConferenceName = request.ConferenceName,
                ConferenceRanking = request.ConferenceRanking,
                Location = request.Location,
                PresentationDate = request.PresentationDate,
                AcceptanceDate = request.AcceptanceDate,
                PresentationType = request.PresentationType,
                ProjectId = request.ProjectId
            };

            await _conferenceRepository.AddAsync(conference);

            // Tạo conference expense
            var expense = new ConferenceExpense
            {
                Accomodation = request.Accomodation,
                AccomodationExpense = request.AccomodationExpense,
                Travel = request.Travel,
                TravelExpense = request.TravelExpense,
                ConferenceId = conference.ConferenceId
            };

            await _conferenceRepository.AddExpenseAsync(expense);

            // Lấy thông tin conference vừa tạo kèm theo project và expense
            var createdConference = await _conferenceRepository.GetConferenceWithDetailsAsync(conference.ConferenceId);
            return _mapper.Map<ConferenceResponse>(createdConference);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating conference: {ex.Message}");
        }
    }
    public async Task<int> CreateConferenceFromResearch(int projectId, int userId, CreateConferenceFromProjectRequest request)
    {
        try
        {
            // Validate conference ranking is in the enum range
            if (!Enum.IsDefined(typeof(ConferenceRankingEnum), request.ConferenceRanking))
                throw new ServiceException($"Invalid conference ranking value: {request.ConferenceRanking}. Valid values are: NotRanked (0), C (1), B (2), A (3), AStar (4)");

            // Validate presentation type is in the enum range
            if (!Enum.IsDefined(typeof(ConferencePresentationTypeEnum), request.PresentationType))
                throw new ServiceException($"Invalid presentation type: {request.PresentationType}. Valid values are: Oral (0), Poster (1), Workshop (2), Panel (3), Virtual (4), Demonstration (5)");

            // The rest of the existing validation code
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ServiceException("Project not found");

            // Check if project status is approved
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Only approved projects can register for conferences");

            // Check if project has at least one completed phase
            if (!project.ProjectPhases.Any(p => p.Status == (int)ProjectPhaseStatusEnum.Completed))
                throw new ServiceException("Project must have at least one completed phase to register for a conference");

            // Check if user is in the project group
            var isUserInGroup = project.Group?.GroupMembers
                .Any(gm => gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active) ?? false;

            if (!isUserInGroup)
                throw new ServiceException("You must be a member of the project group to register for a conference");

            // Create new conference with minimal information
            var conference = new Conference
            {
                ConferenceName = request.ConferenceName,
                ConferenceRanking = request.ConferenceRanking,
                PresentationType = request.PresentationType,
                ProjectId = projectId,
                ConferenceStatus = (int)ConferenceStatusEnum.Active,
                ConferenceSubmissionStatus = (int)ConferenceSubmissionStatusEnum.Pending
            };

            await _context.Conferences.AddAsync(conference);
            await _context.SaveChangesAsync();
            // Notify group members about the conference registration
            if (project.GroupId.HasValue)
            {
                var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
                // Lấy thông tin cần thiết
                var creator = await _context.Users.FindAsync(userId);
                var group = project.Group;
                var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

                // Gửi email và notification cho từng thành viên
                foreach (var member in activeMembers)
                {
                    if (member.UserId.HasValue && member.User != null)
                    {
                        // Gửi email
                        var emailSubject = $"[LRMS] Notification: Project Converted to Conference - {project.ProjectName}";
                        var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                            ? ConferenceEmailTemplates.GetStakeholderConferenceCreationEmail(member.User, project, conference, group, creator)
                            : ConferenceEmailTemplates.GetMemberConferenceCreationEmail(member.User, project, conference, group, creator);

                        await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                        // Gửi notification cho thành viên thường
                        if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                        {
                            var notificationRequest = new CreateNotificationRequest
                            {
                                UserId = member.UserId.Value,
                                Title = "New Conference Registration", // Đã thay đổi
                                Message = $"A new conference '{conference.ConferenceName}' has been registered for project '{project.ProjectName}'", // Đã thay đổi
                                ProjectId = projectId,
                                Status = 0,
                                IsRead = false
                            };
                            await _notificationService.CreateNotification(notificationRequest);
                        }
                    }
                }
                return conference.ConferenceId;
            }
            return conference.ConferenceId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating conference from research: {ex.Message}");
        }
    }

    public async Task<bool> ApproveConference(int conferenceId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new ServiceException("Không tìm thấy conference");

            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền phê duyệt conference");

            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != conference.Project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với conference này");

            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"conferences/{conferenceId}/council-documents");

            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = conference.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            var document = new Document
            {
                ProjectId = conference.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = projectResource.ProjectResourceId
            };
            await _context.Documents.AddAsync(document);

            conference.Project.Status = (int)ProjectStatusEnum.Approved;
            await _context.SaveChangesAsync();

            // Lấy thông tin approver
            var approver = await _context.Users.FindAsync(secretaryId);
            var group = conference.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: Conference Approved - {conference.ConferenceName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? ConferenceEmailTemplates.GetStakeholderConferenceApprovalEmail(member.User, conference.Project, conference, group, approver, documentUrl)
                        : ConferenceEmailTemplates.GetMemberConferenceApprovalEmail(member.User, conference.Project, conference, group, approver, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Conference Approved", // Đã thay đổi
                            Message = $"Conference '{conference.ConferenceName}' has been approved. Please check the council meeting documents for details.", // Đã thay đổi
                            ProjectId = conference.ProjectId.Value,
                            Status = 0,
                            IsRead = false
                        };
                        await _notificationService.CreateNotification(notificationRequest);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Lỗi khi phê duyệt conference: {ex.Message}");
        }
    }

    public async Task<bool> RejectConference(int conferenceId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new ServiceException("Không tìm thấy conference");

            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền từ chối conference");

            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != conference.Project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với conference này");

            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"conferences/{conferenceId}/council-documents");

            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = conference.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            var document = new Document
            {
                ProjectId = conference.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = projectResource.ProjectResourceId
            };
            await _context.Documents.AddAsync(document);

            conference.Project.Status = (int)ProjectStatusEnum.Rejected;
            await _context.SaveChangesAsync();

            // Lấy thông tin
            var group = conference.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: Conference Rejected - {conference.ConferenceName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? ConferenceEmailTemplates.GetStakeholderConferenceRejectionEmail(member.User, conference.Project, conference, group, documentUrl)
                        : ConferenceEmailTemplates.GetMemberConferenceRejectionEmail(member.User, conference.Project, conference, group, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Conference Rejected", // Đã thay đổi
                            Message = $"Conference '{conference.ConferenceName}' has been rejected. Please check the council meeting documents for details.", // Đã thay đổi
                            ProjectId = conference.ProjectId.Value,
                            Status = 0,
                            IsRead = false
                        };
                        await _notificationService.CreateNotification(notificationRequest);
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Lỗi khi từ chối conference: {ex.Message}");
        }
    }

    public async Task<bool> AddConferenceDocument(int conferenceId, int userId, IFormFile documentFile)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new ServiceException("Không tìm thấy conference");

            if (documentFile == null)
                throw new ServiceException("Vui lòng cung cấp file tài liệu");

            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"conferences/{conferenceId}/documents");

            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = conference.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };

            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            var document = new Document
            {
                ProjectId = conference.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.ConferenceProposal,
                UploadAt = DateTime.Now,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            // Lấy thông tin cần thiết
            var uploader = await _context.Users.FindAsync(userId);
            var group = conference.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: New Document for Conference - {conference.ConferenceName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? ConferenceEmailTemplates.GetStakeholderConferenceDocumentEmail(member.User, conference.Project, conference, uploader, documentFile.FileName, documentUrl)
                        : ConferenceEmailTemplates.GetMemberConferenceDocumentEmail(member.User, conference.Project, conference, uploader, documentFile.FileName, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        string title = member.UserId.Value == userId
                            ? "You Have Uploaded New Document" // Đã thay đổi
                            : "New Document in Conference"; // Đã thay đổi

                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = title,
                            Message = $"Project: {conference.ConferenceName}\nDocument: {documentFile.FileName}\nUploaded by: {uploader?.FullName}", // Đã thay đổi
                            ProjectId = conference.ProjectId.Value,
                            Status = 0,
                            IsRead = false
                        };
                        await _notificationService.CreateNotification(notificationRequest);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Lỗi khi thêm tài liệu cho conference: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceResponse>> GetAllConferences()
    {
        try
        {
            var conferences = await _conferenceRepository.GetAllConferencesWithDetailsAsync();
            return _mapper.Map<IEnumerable<ConferenceResponse>>(conferences);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting all conferences: {ex.Message}");
        }
    }

    public async Task<ConferenceResponse> GetConferenceById(int conferenceId)
    {
        try
        {
            var conference = await _conferenceRepository.GetConferenceWithDetailsAsync(conferenceId);
            if (conference == null)
                throw new ServiceException("Conference not found");

            return _mapper.Map<ConferenceResponse>(conference);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting conference by id: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceResponse>> GetConferencesByProjectId(int projectId)
    {
        try
        {
            var conferences = await _conferenceRepository.GetAllConferencesWithDetailsAsync();
            var projectConferences = conferences.Where(c => c.ProjectId == projectId);
            return _mapper.Map<IEnumerable<ConferenceResponse>>(projectConferences);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting conferences by project id: {ex.Message}");
        }
    }

    public async Task AddConferenceDocuments(int conferenceId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            int projectId = conference.ProjectId.Value;
            
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/conferences/{conferenceId}");
                int index = 0;
                
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
                        DocumentType = (int)DocumentTypeEnum.ConferencePaper,
                        UploadAt = DateTime.Now,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        ConferenceId = conferenceId
                    };

                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading conference documents: {ex.Message}");
        }
    }

    public async Task<bool> UpdateConferenceSubmission(int conferenceId, int userId, UpdateConferenceSubmissionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate the conference exists
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify user is authorized (member of the project group)
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            // Also allow lecturers to update
            var user = await _userRepository.GetByIdAsync(userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;
            
            if (!isUserAuthorized && !isLecturer)
                throw new ServiceException("You are not authorized to update this conference");
            
            // Update submission status
            conference.ConferenceSubmissionStatus = request.SubmissionStatus;
            conference.ReviewerComment = request.ReviewerComment;
            
            if (request.SubmissionStatus == (int)ConferenceSubmissionStatusEnum.Approved)
            {
                // Update additional details for approved submissions
                conference.Location = request.Location;
                conference.PresentationDate = request.PresentationDate;
                conference.AcceptanceDate = request.AcceptanceDate;
                conference.ConferenceFunding = request.ConferenceFunding;
                
                _context.Conferences.Update(conference);
                
                // Notify team members about the approval
                await NotifyTeamAboutSubmissionUpdate(conference, true);
            }
            else if (request.SubmissionStatus == (int)ConferenceSubmissionStatusEnum.Rejected)
            {
                // Mark existing conference as rejected
                _context.Conferences.Update(conference);
                
                // Notify team about rejection
                await NotifyTeamAboutSubmissionUpdate(conference, false);
                
                // If new conference details provided, create a new conference entry
                if (!string.IsNullOrEmpty(request.NewConferenceName) && request.NewConferenceRanking.HasValue)
                {
                    var newConference = new Conference
                    {
                        ConferenceName = request.NewConferenceName,
                        ConferenceRanking = request.NewConferenceRanking,
                        PresentationType = conference.PresentationType, // Carry over from previous
                        ProjectId = conference.ProjectId,
                        ConferenceStatus = (int)ConferenceStatusEnum.Active,
                        ConferenceSubmissionStatus = (int)ConferenceSubmissionStatusEnum.Pending
                    };
                    
                    await _context.Conferences.AddAsync(newConference);
                    
                    // Notify team about new submission
                    if (newConference.ProjectId.HasValue && newConference.Project?.GroupId.HasValue == true)
                    {
                        var groupMembers = await _groupRepository.GetMembersByGroupId(newConference.Project.GroupId.Value);
                        foreach (var member in groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active))
                        {
                            if (member.UserId.HasValue)
                            {
                                var notification = new CreateNotificationRequest
                                {
                                    UserId = member.UserId.Value,
                                    Title = "New Conference Submission",
                                    Message = $"A new conference submission has been created for '{request.NewConferenceName}' after previous rejection",
                                    ProjectId = newConference.ProjectId.Value,
                                    Status = 0,
                                    IsRead = false
                                };
                                
                                await _notificationService.CreateNotification(notification);
                            }
                        }
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ServiceException($"Error updating conference submission: {ex.Message}");
        }
    }

    // Helper method for notifications
    private async Task NotifyTeamAboutSubmissionUpdate(Conference conference, bool isApproved)
    {
        if (conference.ProjectId.HasValue && conference.Project?.GroupId.HasValue == true)
        {
            var groupMembers = await _groupRepository.GetMembersByGroupId(conference.Project.GroupId.Value);
            var activeMembers = groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);
            
            string title = isApproved ? "Conference Submission Approved" : "Conference Submission Rejected";
            string message = isApproved 
                ? $"Submission to conference '{conference.ConferenceName}' has been approved!"
                : $"Submission to conference '{conference.ConferenceName}' has been rejected. Reason: {conference.ReviewerComment}";
            
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue)
                {
                    var notification = new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = title,
                        Message = message,
                        ProjectId = conference.ProjectId.Value,
                        Status = 0,
                        IsRead = false
                    };
                    
                    await _notificationService.CreateNotification(notification);
                }
            }
        }
    }

    public async Task<bool> UpdateConferenceStatus(int conferenceId, int userId, int conferenceStatus)
    {
        try
        {
            if (!Enum.IsDefined(typeof(ConferenceStatusEnum), conferenceStatus))
                throw new ServiceException("Invalid conference status value");
            
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify user is authorized (member of the project group)
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            // Also allow lecturers to update
            var user = await _userRepository.GetByIdAsync(userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;
            
            if (!isUserAuthorized && !isLecturer)
                throw new ServiceException("You are not authorized to update this conference status");
            
            // Update the status
            conference.ConferenceStatus = conferenceStatus;
            
            _context.Conferences.Update(conference);
            await _context.SaveChangesAsync();
            
            // Notify team about status change
            string statusName = Enum.GetName(typeof(ConferenceStatusEnum), conferenceStatus) ?? conferenceStatus.ToString();
            
            if (conference.ProjectId.HasValue && conference.Project?.GroupId.HasValue == true)
            {
                var groupMembers = await _groupRepository.GetMembersByGroupId(conference.Project.GroupId.Value);
                
                foreach (var member in groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active))
                {
                    if (member.UserId.HasValue)
                    {
                        var notification = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Conference Status Updated",
                            Message = $"Status for conference '{conference.ConferenceName}' has been changed to {statusName}",
                            ProjectId = conference.ProjectId.Value,
                            Status = 0,
                            IsRead = false
                        };
                        
                        await _notificationService.CreateNotification(notification);
                    }
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating conference status: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceResponse>> GetUserConferences(int userId)
    {
        try
        {
            // Get all groups where the user is a member
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            if (!userGroups.Any())
                return Enumerable.Empty<ConferenceResponse>();
            
            var groupIds = userGroups.Select(g => g.GroupId).ToList();
            
            // Get all projects for these groups
            var projects = await _context.Projects
                .Where(p => p.GroupId.HasValue && groupIds.Contains(p.GroupId.Value))
                .Select(p => p.ProjectId)
                .ToListAsync();
            
            if (!projects.Any())
                return Enumerable.Empty<ConferenceResponse>();
            
            // Get all conferences for these projects
            var conferences = await _context.Conferences
                .Include(c => c.Project)
                .Include(c => c.ConferenceExpenses)
                .Where(c => c.ProjectId.HasValue && projects.Contains(c.ProjectId.Value))
                .ToListAsync();
            
            // Map to response objects
            var responses = _mapper.Map<IEnumerable<ConferenceResponse>>(conferences).ToList();
            
            foreach (var response in responses)
            {
                // Get conference entity
                var conference = conferences.FirstOrDefault(c => c.ConferenceId == response.ConferenceId);
                if (conference != null)
                {
                    // Populate ranking name
                    response.ConferenceRankingName = GetConferenceRankingDisplayName(conference.ConferenceRanking);
                    
                    // Populate status information
                    response.ConferenceStatus = conference.ConferenceStatus ?? (int)ConferenceStatusEnum.Active;
                    response.ConferenceStatusName = Enum.GetName(typeof(ConferenceStatusEnum), 
                        conference.ConferenceStatus ?? (int)ConferenceStatusEnum.Active) ?? "Unknown";
                    
                    // Populate submission status
                    response.ConferenceSubmissionStatus = conference.ConferenceSubmissionStatus ?? 
                        (int)ConferenceSubmissionStatusEnum.Pending;
                    response.ConferenceSubmissionStatusName = Enum.GetName(typeof(ConferenceSubmissionStatusEnum), 
                        conference.ConferenceSubmissionStatus ?? (int)ConferenceSubmissionStatusEnum.Pending) ?? "Unknown";
                }
            }
            
            return responses;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving user conferences: {ex.Message}", ex);
        }
    }

    public async Task<ConferenceDetailResponse> GetConferenceDetails(int conferenceId)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                .Include(c => c.ConferenceExpenses)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException($"Conference with ID {conferenceId} not found");
            
            // Get documents for this conference
            var documents = await _context.Documents
                .Where(d => d.ConferenceId == conferenceId)
                .ToListAsync();
            
            var response = new ConferenceDetailResponse
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                ConferenceRanking = conference.ConferenceRanking ?? 0,
                ConferenceRankingName = GetConferenceRankingDisplayName(conference.ConferenceRanking),
                Location = conference.Location,
                PresentationDate = conference.PresentationDate,
                AcceptanceDate = conference.AcceptanceDate,
                PresentationType = conference.PresentationType ?? 0,
                PresentationTypeName = GetPresentationTypeDisplayName(conference.PresentationType),
                
                // Status information
                ConferenceStatus = conference.ConferenceStatus ?? (int)ConferenceStatusEnum.Active,
                ConferenceStatusName = Enum.GetName(typeof(ConferenceStatusEnum), conference.ConferenceStatus ?? 0) ?? "Unknown",
                ConferenceSubmissionStatus = conference.ConferenceSubmissionStatus ?? (int)ConferenceSubmissionStatusEnum.Pending,
                ConferenceSubmissionStatusName = Enum.GetName(typeof(ConferenceSubmissionStatusEnum), conference.ConferenceSubmissionStatus ?? 0) ?? "Unknown",
                ConferenceFunding = conference.ConferenceFunding,
                ReviewerComment = conference.ReviewerComment,
                
                // Project information
                ProjectId = conference.ProjectId ?? 0,
                ProjectName = conference.Project?.ProjectName ?? "Unknown",
                ProjectStatus = conference.Project?.Status ?? 0,
                
                // Documents
                Documents = _mapper.Map<List<DocumentResponse>>(documents)
            };
            
            // Expense information
            if (conference.ConferenceExpenses.Any())
            {
                var expense = conference.ConferenceExpenses.First();
                response.Expense = _mapper.Map<ConferenceExpenseResponse>(expense);
                
                // Handle rejection reasons for rejected expenses
                if (expense.ExpenseStatus == (int)ConferenceExpenseStatusEnum.Rejected)
                {
                    // Check if there's a direct fund disbursement relationship
                    var fundDisbursement = await _context.FundDisbursements
                        .FirstOrDefaultAsync(fd => fd.ExpenseId == expense.ExpenseId &&
                                           fd.Status == (int)FundDisbursementStatusEnum.Rejected);
                        
                    if (fundDisbursement != null && !string.IsNullOrEmpty(fundDisbursement.RejectionReason))
                    {
                        response.Expense.RejectionReason = fundDisbursement.RejectionReason;
                    }
                    else
                    {
                        // Fallback to project request rejection reason
                        var projectRequest = await _context.ProjectRequests
                            .FirstOrDefaultAsync(pr => pr.FundDisbursementId.HasValue && 
                                               pr.FundDisbursement.ExpenseId == expense.ExpenseId &&
                                               pr.ApprovalStatus == ApprovalStatusEnum.Rejected);
                            
                        if (projectRequest != null && !string.IsNullOrEmpty(projectRequest.RejectionReason))
                        {
                            response.Expense.RejectionReason = projectRequest.RejectionReason;
                        }
                    }
                }
            }
            
            return response;
        }
        catch (Exception ex) when (ex is not ServiceException)
        {
            throw new ServiceException($"Error retrieving conference details: {ex.Message}", ex);
        }
    }

    private string GetConferenceRankingDisplayName(int? ranking)
    {
        if (!ranking.HasValue)
            return "Not Ranked";
        
        return (ConferenceRankingEnum)ranking.Value switch
        {
            ConferenceRankingEnum.NotRanked => "Not Ranked",
            ConferenceRankingEnum.C => "C",
            ConferenceRankingEnum.B => "B",
            ConferenceRankingEnum.A => "A",
            ConferenceRankingEnum.AStar => "A*",
            _ => "Unknown"
        };
    }

    private string GetPresentationTypeDisplayName(int? presentationType)
    {
        if (!presentationType.HasValue)
            return "Unknown";
        
        return (ConferencePresentationTypeEnum)presentationType.Value switch
        {
            ConferencePresentationTypeEnum.OralPresentation => "Oral Presentation",
            ConferencePresentationTypeEnum.PosterPresentation => "Poster Presentation",
            ConferencePresentationTypeEnum.WorkshopPresentation => "Workshop Presentation",
            ConferencePresentationTypeEnum.PanelPresentation => "Panel Presentation",
            ConferencePresentationTypeEnum.VirtualPresentation => "Virtual Presentation",
            ConferencePresentationTypeEnum.Demonstration => "Demonstration",
            _ => "Unknown"
        };
    }

    public async Task<bool> UpdateSubmissionStatus(int conferenceId, int userId, int submissionStatus, string reviewerComment)
    {
        try
        {
            // Validate submission status
            if (!Enum.IsDefined(typeof(ConferenceSubmissionStatusEnum), submissionStatus))
                throw new ServiceException("Invalid submission status value");
            
            // Get conference
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify user is authorized
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            var user = await _userRepository.GetByIdAsync(userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;
            
            if (!isUserAuthorized && !isLecturer)
                throw new ServiceException("You are not authorized to update this conference");
            
            // Update submission status
            conference.ConferenceSubmissionStatus = submissionStatus;
            conference.ReviewerComment = reviewerComment;
            
            _context.Conferences.Update(conference);
            await _context.SaveChangesAsync();
            
            // Notify team about status change
            await NotifyTeamAboutSubmissionUpdate(conference, submissionStatus == (int)ConferenceSubmissionStatusEnum.Approved);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating submission status: {ex.Message}");
        }
    }

    public async Task<bool> UpdateApprovedConferenceDetails(int conferenceId, int userId, UpdateApprovedConferenceRequest request)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify conference is approved
            if (conference.ConferenceSubmissionStatus != (int)ConferenceSubmissionStatusEnum.Approved)
                throw new ServiceException("Cannot update details - conference submission is not approved");
            
            // Verify user is authorized
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            var user = await _userRepository.GetByIdAsync(userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;
            
            if (!isUserAuthorized && !isLecturer)
                throw new ServiceException("You are not authorized to update this conference");
            
            // Update conference details
            conference.Location = request.Location;
            conference.PresentationDate = request.PresentationDate;
            conference.AcceptanceDate = request.AcceptanceDate;
            conference.ConferenceFunding = request.ConferenceFunding;
            
            _context.Conferences.Update(conference);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating conference details: {ex.Message}");
        }
    }

    public async Task<int> CreateNewSubmissionAfterRejection(int projectId, int userId, int rejectedConferenceId, CreateNewSubmissionRequest request)
    {
        try
        {
            // Get the rejected conference to check if it exists and is rejected
            var rejectedConference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == rejectedConferenceId);
            
            if (rejectedConference == null)
                throw new ServiceException("Rejected conference not found");
            
            if (rejectedConference.ConferenceSubmissionStatus != (int)ConferenceSubmissionStatusEnum.Rejected)
                throw new ServiceException("Previous conference submission is not rejected");
            
            if (rejectedConference.ProjectId != projectId)
                throw new ServiceException("Conference does not belong to the specified project");
            
            // Verify user is authorized
            bool isUserAuthorized = rejectedConference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            var user = await _userRepository.GetByIdAsync(userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;
            
            if (!isUserAuthorized && !isLecturer)
                throw new ServiceException("You are not authorized to create a new submission");
            
            // Create new conference for the same project
            var newConference = new Conference
            {
                ConferenceName = request.ConferenceName,
                ConferenceRanking = request.ConferenceRanking,
                PresentationType = rejectedConference.PresentationType, // Copy from rejected conference
                ProjectId = projectId,
                ConferenceStatus = (int)ConferenceStatusEnum.Active,
                ConferenceSubmissionStatus = (int)ConferenceSubmissionStatusEnum.Pending,
                ReviewerComment = request.ReviewerComment
            };
            
            await _context.Conferences.AddAsync(newConference);
            await _context.SaveChangesAsync();
            
            // Notify team about new submission
            if (newConference.ProjectId.HasValue && newConference.Project?.GroupId.HasValue == true)
            {
                var groupMembers = await _groupRepository.GetMembersByGroupId(newConference.Project.GroupId.Value);
                foreach (var member in groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active))
                {
                    if (member.UserId.HasValue)
                    {
                        var notification = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "New Conference Submission",
                            Message = $"A new conference submission has been created for '{request.ConferenceName}' after previous rejection",
                            ProjectId = newConference.ProjectId.Value,
                            Status = 0,
                            IsRead = false
                        };
                        
                        await _notificationService.CreateNotification(notification);
                    }
                }
            }
            
            return newConference.ConferenceId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating new submission: {ex.Message}");
        }
    }

    public async Task<int> RequestConferenceExpenseAsync(int userId, RequestConferenceExpenseRequest request, IEnumerable<IFormFile> documentFiles)
    {
        try
        {
            // Get the conference and verify it exists
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == request.ConferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify conference is approved
            if (conference.ConferenceSubmissionStatus != (int)ConferenceSubmissionStatusEnum.Approved)
                throw new ServiceException("Cannot request expenses - conference submission must be approved first");
            
            // Verify user is authorized (member of the project group)
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            if (!isUserAuthorized)
                throw new ServiceException("You are not authorized to request expenses for this conference");
            
            // Check for existing expense requests for this conference by querying ProjectRequests directly
            var existingFundDisbursements = await _context.FundDisbursements
                .Where(fd => fd.ConferenceId == conference.ConferenceId && 
                             fd.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceExpense)
                .ToListAsync();
            
            if (existingFundDisbursements.Any())
            {
                var fundDisbursementIds = existingFundDisbursements.Select(fd => fd.FundDisbursementId).ToList();
                
                // Check ProjectRequests to determine the true status
                var existingRequests = await _context.ProjectRequests
                    .Where(pr => pr.FundDisbursementId.HasValue && 
                               fundDisbursementIds.Contains(pr.FundDisbursementId.Value))
                    .ToListAsync();
                
                // Check for approved requests - if any are approved, no new requests allowed
                bool hasApproved = existingRequests.Any(pr => pr.ApprovalStatus == ApprovalStatusEnum.Approved);
                if (hasApproved)
                    throw new ServiceException("An expense request for this conference has already been approved. No additional expense requests can be made.");
                    
                // More direct query to check for pending conference expense requests
                var hasPendingExpenseRequest = await _context.ProjectRequests
                    .AnyAsync(pr => pr.FundDisbursementId.HasValue && 
                              pr.RequestType == ProjectRequestTypeEnum.Fund_Disbursement &&
                              pr.ApprovalStatus == ApprovalStatusEnum.Pending &&
                              _context.FundDisbursements.Any(fd => 
                                  fd.FundDisbursementId == pr.FundDisbursementId &&
                                  fd.ConferenceId == conference.ConferenceId &&
                                  fd.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceExpense));

                if (hasPendingExpenseRequest)
                    throw new ServiceException("There is already a pending expense request for this conference. Please wait for it to be processed.");
                    
                // If we're here, all previous requests were rejected, so we can continue
            }
            
            // Create conference expense with PENDING status
            var expense = new ConferenceExpense
            {
                ConferenceId = conference.ConferenceId,
                Accomodation = request.Accommodation,
                AccomodationExpense = request.AccommodationExpense,
                Travel = request.Travel,
                TravelExpense = request.TravelExpense,
                ExpenseStatus = (int)ConferenceExpenseStatusEnum.Pending
            };
            
            await _context.ConferenceExpenses.AddAsync(expense);
            await _context.SaveChangesAsync();
            
            // Create fund disbursement with type ConferenceExpense
            var fundDisbursement = new FundDisbursement
            {
                FundRequest = request.AccommodationExpense + request.TravelExpense,
                ProjectId = conference.ProjectId ?? 0,
                UserRequest = userId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)FundDisbursementStatusEnum.Pending,
                Description = $"Conference expense request for {conference.ConferenceName}",
                ConferenceId = conference.ConferenceId,
                FundDisbursementType = (int)FundDisbursementTypeEnum.ConferenceExpense,
                ExpenseId = expense.ExpenseId  
            };
            
            await _context.FundDisbursements.AddAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Create project request with type Fund_Disbursement
            var expenseRequest = new ProjectRequest
            {
                ProjectId = conference.ProjectId ?? 0,
                RequestType = ProjectRequestTypeEnum.Fund_Disbursement, // Changed from Conference_Expense to Fund_Disbursement
                RequestedById = userId,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatusEnum.Pending,
                FundDisbursementId = fundDisbursement.FundDisbursementId
            };
            
            await _context.ProjectRequests.AddAsync(expenseRequest);
            await _context.SaveChangesAsync();
            
            // Upload documents if provided
            if (documentFiles != null && documentFiles.Any())
            {
                string folderPath = $"projects/{conference.ProjectId}/conferences/{conference.ConferenceId}/expense-requests";
                var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create resource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document
                        ProjectId = conference.ProjectId ?? 0,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    // Create document
                    var document = new Document
                    {
                        ProjectId = conference.ProjectId ?? 0,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.ConferenceExpense, // Use ConferenceExpense document type (8)
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        FundDisbursementId = fundDisbursement.FundDisbursementId,
                        RequestId = expenseRequest.RequestId,
                        ConferenceId = conference.ConferenceId
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            // Notify office users
            var officeUsers = await _context.Users
                .Where(u => u.Role == (int)SystemRoleEnum.Office)
                .ToListAsync();
            
            foreach (var office in officeUsers)
            {
                var notification = new CreateNotificationRequest
                {
                    UserId = office.UserId,
                    Title = "New Conference Expense Request",
                    Message = $"A new conference expense request has been submitted for '{conference.ConferenceName}' (Total: {fundDisbursement.FundRequest:C})",
                    ProjectId = conference.ProjectId.Value,
                    Status = 0,
                    IsRead = false
                };
                
                await _notificationService.CreateNotification(notification);
            }
            
            return expenseRequest.RequestId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error requesting conference expenses: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceExpenseRequestResponse>> GetPendingConferenceExpenseRequestsAsync()
    {
        try
        {
            // Find all fund disbursement requests that are for conference expenses
            var projectRequests = await _context.ProjectRequests
                .Include(r => r.Project)
                .Include(r => r.RequestedBy)
                .Where(r => r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement && 
                       r.FundDisbursementId.HasValue)
                .ToListAsync();
            
            var response = new List<ConferenceExpenseRequestResponse>();
            
            foreach (var request in projectRequests)
            {
                var fundDisbursement = await _context.FundDisbursements
                    .FirstOrDefaultAsync(fd => fd.FundDisbursementId == request.FundDisbursementId);
                    
                if (fundDisbursement == null || 
                    fundDisbursement.FundDisbursementType != (int)FundDisbursementTypeEnum.ConferenceExpense || 
                    !fundDisbursement.ConferenceId.HasValue)
                    continue;
                    
                var conference = await _context.Conferences
                    .FirstOrDefaultAsync(c => c.ConferenceId == fundDisbursement.ConferenceId);
                    
                if (conference == null)
                    continue;
                    
                var expenses = await _context.ConferenceExpenses
                    .Where(ce => ce.ConferenceId == conference.ConferenceId)
                    .OrderByDescending(ce => ce.ExpenseId)
                    .ToListAsync();
                    
                if (!expenses.Any())
                    continue;
                    
                var expense = expenses.First();
                
                // Get rejection reason from either fund disbursement or project request
                string rejectionReason = null;
                if (request.ApprovalStatus == ApprovalStatusEnum.Rejected)
                {
                    rejectionReason = !string.IsNullOrEmpty(fundDisbursement.RejectionReason) 
                        ? fundDisbursement.RejectionReason 
                        : request.RejectionReason;
                }
                
                response.Add(new ConferenceExpenseRequestResponse
                {
                    RequestId = request.RequestId,
                    ProjectId = request.ProjectId,
                    ProjectName = request.Project?.ProjectName ?? "Unknown",
                    RequestedById = request.RequestedById,
                    RequesterName = request.RequestedBy?.FullName ?? "Unknown",
                    RequestedAt = request.RequestedAt,
                    
                    ConferenceId = conference.ConferenceId,
                    ConferenceName = conference.ConferenceName,
                    
                    FundDisbursementId = fundDisbursement.FundDisbursementId,
                    TotalAmount = fundDisbursement.FundRequest ?? 0,
                    
                    AccommodationDetails = expense.Accomodation,
                    AccommodationAmount = expense.AccomodationExpense ?? 0,
                    TravelDetails = expense.Travel,
                    TravelAmount = expense.TravelExpense ?? 0,
                    
                    ExpenseStatus = expense.ExpenseStatus ?? 0,
                    ExpenseStatusName = Enum.GetName(typeof(ConferenceExpenseStatusEnum), expense.ExpenseStatus ?? 0) ?? "Unknown",
                    FundDisbursementType = fundDisbursement.FundDisbursementType,
                    FundDisbursementTypeName = fundDisbursement.FundDisbursementType.HasValue ? 
                        Enum.GetName(typeof(FundDisbursementTypeEnum), fundDisbursement.FundDisbursementType.Value) : "Unknown",
                    
                    // Add rejection reason
                    RejectionReason = rejectionReason
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving pending conference expense requests: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceExpenseResponse>> GetConferenceExpensesAsync(int conferenceId)
    {
        try
        {
            // Get the conference and verify it exists
            var conference = await _context.Conferences
                .Include(c => c.ConferenceExpenses)
                    .ThenInclude(ce => ce.FundDisbursement) // Include the linked fund disbursement 
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException($"Conference with ID {conferenceId} not found");
            
            if (!conference.ConferenceExpenses.Any())
                return new List<ConferenceExpenseResponse>();
            
            // Map all expenses to response objects
            var responses = new List<ConferenceExpenseResponse>();
            
            foreach (var expense in conference.ConferenceExpenses)
            {
                var response = _mapper.Map<ConferenceExpenseResponse>(expense);
                
                // Add status name if missing
                if (expense.ExpenseStatus.HasValue)
                {
                    response.ExpenseStatusName = Enum.GetName(typeof(ConferenceExpenseStatusEnum), expense.ExpenseStatus.Value) ?? "Unknown";
                }
                
                // Handle rejection reasons for rejected expenses
                if (expense.ExpenseStatus == (int)ConferenceExpenseStatusEnum.Rejected)
                {
                    // Directly use the linked fund disbursement (navigation property)
                    if (expense.FundDisbursement != null && !string.IsNullOrEmpty(expense.FundDisbursement.RejectionReason))
                    {
                        response.RejectionReason = expense.FundDisbursement.RejectionReason;
                    }
                    else
                    {
                        // Fallback to query in case the navigation property wasn't loaded
                        var fundDisbursement = await _context.FundDisbursements
                            .FirstOrDefaultAsync(fd => fd.ExpenseId == expense.ExpenseId);
                            
                        if (fundDisbursement != null && !string.IsNullOrEmpty(fundDisbursement.RejectionReason))
                        {
                            response.RejectionReason = fundDisbursement.RejectionReason;
                        }
                    }
                }
                
                responses.Add(response);
            }
            
            return responses;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving conference expenses: {ex.Message}");
        }
    }

    public async Task<int> RequestConferenceFundingAsync(int userId, RequestConferenceFundingRequest request, IEnumerable<IFormFile> documentFiles)
    {
        try
        {
            // Get the conference and verify it exists
            var conference = await _context.Conferences
                .Include(c => c.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(c => c.ConferenceId == request.ConferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            // Verify conference is approved
            if (conference.ConferenceSubmissionStatus != (int)ConferenceSubmissionStatusEnum.Approved)
                throw new ServiceException("Cannot request funding - conference submission must be approved first");
            
            // Verify user is authorized (member of the project group)
            bool isUserAuthorized = conference.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            if (!isUserAuthorized)
                throw new ServiceException("You are not authorized to request funding for this conference");
            
            // Check for existing conference funding requests
            var existingFundDisbursements = await _context.FundDisbursements
                .Where(fd => fd.ConferenceId == conference.ConferenceId && 
                             fd.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceFunding)
                .ToListAsync();

            if (existingFundDisbursements.Any())
            {
                var fundDisbursementIds = existingFundDisbursements.Select(fd => fd.FundDisbursementId).ToList();
                
                // Check ProjectRequests to determine the true status
                var existingRequests = await _context.ProjectRequests
                    .Where(pr => pr.FundDisbursementId.HasValue && 
                               fundDisbursementIds.Contains(pr.FundDisbursementId.Value))
                    .ToListAsync();
                
                // Check for approved requests - if any are approved, no new requests allowed
                bool hasApproved = existingRequests.Any(pr => pr.ApprovalStatus == ApprovalStatusEnum.Approved);
                if (hasApproved)
                    throw new ServiceException("A funding request for this conference has already been approved. No additional funding requests can be made.");
                
                // Check for pending requests - if any are pending, no new requests allowed
                bool hasPending = existingRequests.Any(pr => pr.ApprovalStatus == ApprovalStatusEnum.Pending);
                if (hasPending)
                    throw new ServiceException("There is already a pending funding request for this conference. Please wait for it to be processed.");
                
                // If we're here, all previous requests were rejected, so we can continue
            }
            
            // Update conference details
            conference.Location = request.Location;
            conference.PresentationDate = request.PresentationDate;
            conference.AcceptanceDate = request.AcceptanceDate;
            conference.ConferenceFunding = request.ConferenceFunding;
            
            _context.Conferences.Update(conference);
            await _context.SaveChangesAsync();
            
            // Create fund disbursement with type ConferenceFunding
            var fundDisbursement = new FundDisbursement
            {
                FundRequest = request.ConferenceFunding,
                ProjectId = conference.ProjectId ?? 0,
                UserRequest = userId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)FundDisbursementStatusEnum.Pending,
                Description = $"Conference funding request for {conference.ConferenceName}",
                ConferenceId = conference.ConferenceId,
                FundDisbursementType = (int)FundDisbursementTypeEnum.ConferenceFunding
            };
            
            await _context.FundDisbursements.AddAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Create project request with type Fund_Disbursement
            var fundingRequest = new ProjectRequest
            {
                ProjectId = conference.ProjectId ?? 0,
                RequestType = ProjectRequestTypeEnum.Fund_Disbursement,
                RequestedById = userId,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatusEnum.Pending,
                FundDisbursementId = fundDisbursement.FundDisbursementId
            };
            
            await _context.ProjectRequests.AddAsync(fundingRequest);
            await _context.SaveChangesAsync();
            
            // Upload documents if provided
            if (documentFiles != null && documentFiles.Any())
            {
                string folderPath = $"projects/{conference.ProjectId}/conferences/{conference.ConferenceId}/funding";
                var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create resource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document
                        ProjectId = conference.ProjectId ?? 0,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    // Create document
                    var document = new Document
                    {
                        ProjectId = conference.ProjectId ?? 0,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.ConferenceFunding,
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        FundDisbursementId = fundDisbursement.FundDisbursementId,
                        RequestId = fundingRequest.RequestId,
                        ConferenceId = conference.ConferenceId
                    };
                    
                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
            
            // Notify office users
            var officeUsers = await _context.Users
                .Where(u => u.Role == (int)SystemRoleEnum.Office)
                .ToListAsync();
            
            foreach (var office in officeUsers)
            {
                var notification = new CreateNotificationRequest
                {
                    UserId = office.UserId,
                    Title = "New Conference Funding Request",
                    Message = $"A new conference funding request has been submitted for '{conference.ConferenceName}' (Amount: {fundDisbursement.FundRequest:C})",
                    ProjectId = conference.ProjectId.Value,
                    Status = 0,
                    IsRead = false
                };
                
                await _notificationService.CreateNotification(notification);
            }
            
            return fundingRequest.RequestId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error requesting conference funding: {ex.Message}");
        }
    }

    public async Task<bool> UpdateConferenceExpenseStatus(int requestId, int newStatus, string rejectionReason = null, 
        IEnumerable<IFormFile> documentFiles = null, int approverId = 0)
    {
        try
        {
            // Find the project request
            var request = await _context.ProjectRequests
                .Include(r => r.RequestedBy)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement);
            
            if (request == null)
                throw new ServiceException("Fund disbursement request not found");
            
            // Find the associated fund disbursement
            var fundDisbursement = await _context.FundDisbursements
                .FirstOrDefaultAsync(fd => fd.FundDisbursementId == request.FundDisbursementId);
            
            if (fundDisbursement == null)
                throw new ServiceException("Associated fund disbursement not found");
            
            // Check if this is a conference expense or funding request
            if (!fundDisbursement.ConferenceId.HasValue)
                throw new ServiceException("The fund disbursement is not for a conference");
            
            // Get the conference
            var conference = await _context.Conferences
                .FirstOrDefaultAsync(c => c.ConferenceId == fundDisbursement.ConferenceId.Value);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            bool isConferenceExpense = fundDisbursement.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceExpense;
            bool isConferenceFunding = fundDisbursement.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceFunding;
            
            if (!isConferenceExpense && !isConferenceFunding)
                throw new ServiceException("The fund disbursement is not for a conference expense or funding");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Update expense status if this is an expense request
                if (isConferenceExpense && fundDisbursement.ExpenseId.HasValue)
                {
                    // Get the direct linked expense
                    var expenseToUpdate = await _context.ConferenceExpenses
                        .FirstOrDefaultAsync(ce => ce.ExpenseId == fundDisbursement.ExpenseId);
                    
                    if (expenseToUpdate == null)
                        throw new ServiceException("Conference expense record not found");
                    
                    // Update the expense status
                    expenseToUpdate.ExpenseStatus = newStatus;
                    _context.ConferenceExpenses.Update(expenseToUpdate);
                }
                
                // Update fund disbursement status
                fundDisbursement.Status = newStatus == (int)ConferenceExpenseStatusEnum.Approved 
                    ? (int)FundDisbursementStatusEnum.Approved 
                    : (int)FundDisbursementStatusEnum.Rejected;
                
                // Set rejection reason if status is rejected and reason provided
                if (newStatus == (int)ConferenceExpenseStatusEnum.Rejected && !string.IsNullOrWhiteSpace(rejectionReason))
                {
                    fundDisbursement.RejectionReason = rejectionReason;
                    // Also make sure to set rejection reason in the project request
                    request.RejectionReason = rejectionReason;
                    _context.ProjectRequests.Update(request);
                }
                
                _context.FundDisbursements.Update(fundDisbursement);
                
                // Update project request status
                request.ApprovalStatus = newStatus == (int)ConferenceExpenseStatusEnum.Approved 
                    ? ApprovalStatusEnum.Approved 
                    : ApprovalStatusEnum.Rejected;
                
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedById = approverId;
                _context.ProjectRequests.Update(request);
                
                // Special handling for approval based on the type
                if (newStatus == (int)ConferenceExpenseStatusEnum.Approved)
                {
                    if (isConferenceExpense)
                    {
                        // For expense, we add to the quota allocation (increase project's budget)
                        await HandleConferenceExpenseApproval(fundDisbursement, approverId);
                    }
                    else if (isConferenceFunding)
                    {
                        // For funding, we also add to the quota allocation (increase project's budget)
                        await HandleConferenceFundingApproval(fundDisbursement, approverId);
                    }
                }
                
                // Process document uploads if provided
                if (documentFiles != null && documentFiles.Any())
                {
                    // Set the appropriate document type based on approval/rejection
                    var documentType = newStatus == (int)ConferenceExpenseStatusEnum.Approved
                        ? (int)DocumentTypeEnum.ConferenceExpenseDecision
                        : (int)DocumentTypeEnum.ConferenceExpenseDecision;
                    
                    // Upload documents to S3
                    string subType = isConferenceExpense ? "expense" : "funding";
                    string folderPath = newStatus == (int)ConferenceExpenseStatusEnum.Approved
                        ? $"projects/{fundDisbursement.ProjectId}/conferences/{conference.ConferenceId}/{subType}/approval"
                        : $"projects/{fundDisbursement.ProjectId}/conferences/{conference.ConferenceId}/{subType}/rejection";
                    
                    var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
                    int index = 0;
                    
                    foreach (var file in documentFiles)
                    {
                        // Create resource
                        var projectResource = new ProjectResource
                        {
                            ResourceName = file.FileName,
                            ResourceType = 1, // Document
                            ProjectId = fundDisbursement.ProjectId,
                            Acquired = true,
                            Quantity = 1
                        };
                        
                        await _context.ProjectResources.AddAsync(projectResource);
                        await _context.SaveChangesAsync();
                        
                        // Create document
                        var document = new Document
                        {
                            ProjectId = fundDisbursement.ProjectId,
                            DocumentUrl = urls[index],
                            FileName = file.FileName,
                            DocumentType = documentType,
                            UploadAt = DateTime.UtcNow,
                            UploadedBy = approverId,
                            ProjectResourceId = projectResource.ProjectResourceId,
                            FundDisbursementId = fundDisbursement.FundDisbursementId,
                            RequestId = requestId,
                            ConferenceId = conference.ConferenceId
                        };
                        
                        await _context.Documents.AddAsync(document);
                        index++;
                    }
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                // Notify the requester about the approval/rejection
                if (request.RequestedBy != null)
                {
                    var statusText = newStatus == (int)ConferenceExpenseStatusEnum.Approved ? "approved" : "rejected";
                    var reason = newStatus == (int)ConferenceExpenseStatusEnum.Rejected && !string.IsNullOrEmpty(rejectionReason)
                        ? $" Reason: {rejectionReason}"
                        : "";
                    
                    var requestType = isConferenceExpense ? "conference expense" : "conference funding";
                    
                    var notification = new CreateNotificationRequest
                    {
                        UserId = request.RequestedById,
                        Title = $"Conference {requestType} Request {(newStatus == (int)ConferenceExpenseStatusEnum.Approved ? "Approved" : "Rejected")}",
                        Message = $"Your {requestType} request for {conference.ConferenceName} has been {statusText}.{reason}",
                        ProjectId = request.ProjectId,
                        Status = 0,
                        IsRead = false
                    };
                    
                    await _notificationService.CreateNotification(notification);
                    
                    // Also send email notification
                    if (_emailService != null && !string.IsNullOrEmpty(request.RequestedBy.Email))
                    {
                        string emailBody = $"Dear {request.RequestedBy.FullName},\n\n" +
                                          $"Your {requestType} request for {conference.ConferenceName} has been {statusText}.{reason}\n\n" +
                                         $"Total amount: {fundDisbursement.FundRequest:C}\n\n" +
                                         $"This is an automated message. Please do not reply to this email.";
                                         
                        await _emailService.SendEmailAsync(
                            request.RequestedBy.Email,
                            $"Conference {requestType} Request {(newStatus == (int)ConferenceExpenseStatusEnum.Approved ? "Approved" : "Rejected")}",
                            emailBody
                        );
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ServiceException($"Error during transaction: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error updating conference request status: {ex.Message}");
        }
    }

    // Helper method to handle conference expense approval and quota management
    private async Task HandleConferenceExpenseApproval(FundDisbursement fundDisbursement, int approverId)
    {
        // Find the active quota for this project
        var quota = await _context.Quotas
            .FirstOrDefaultAsync(q => q.ProjectId == fundDisbursement.ProjectId && 
                                      q.Status == (int)QuotaStatusEnum.Active);
        
        if (quota == null)
        {
            // If no active quota exists, create a new one
            quota = new Quota
            {
                ProjectId = fundDisbursement.ProjectId,
                AllocatedBudget = fundDisbursement.FundRequest,
                Status = (int)QuotaStatusEnum.Active,
                CreatedAt = DateTime.UtcNow,
                AllocatedBy = approverId
            };
            
            await _context.Quotas.AddAsync(quota);
        }
        else
        {
            // If an active quota exists, increase its allocation
            quota.AllocatedBudget += fundDisbursement.FundRequest;
            quota.UpdateAt = DateTime.UtcNow;
            
            _context.Quotas.Update(quota);
        }
        
        // Link the fund disbursement to this quota
        fundDisbursement.QuotaId = quota.QuotaId;
        _context.FundDisbursements.Update(fundDisbursement);
        
        await _context.SaveChangesAsync();
    }

    // Helper method to handle conference funding approval and quota management
    private async Task HandleConferenceFundingApproval(FundDisbursement fundDisbursement, int approverId)
    {
        // Find the active quota for this project
        var quota = await _context.Quotas
            .FirstOrDefaultAsync(q => q.ProjectId == fundDisbursement.ProjectId && 
                                      q.Status == (int)QuotaStatusEnum.Active);
        
        if (quota == null)
        {
            // If no active quota exists, create a new one
            quota = new Quota
            {
                ProjectId = fundDisbursement.ProjectId,
                AllocatedBudget = fundDisbursement.FundRequest,
                Status = (int)QuotaStatusEnum.Active,
                CreatedAt = DateTime.UtcNow,
                AllocatedBy = approverId
            };
            
            await _context.Quotas.AddAsync(quota);
        }
        else
        {
            // If an active quota exists, increase its allocation
            quota.AllocatedBudget += fundDisbursement.FundRequest;
            quota.UpdateAt = DateTime.UtcNow;
            
            _context.Quotas.Update(quota);
        }
        
        // Link the fund disbursement to this quota
        fundDisbursement.QuotaId = quota.QuotaId;
        _context.FundDisbursements.Update(fundDisbursement);
        
        await _context.SaveChangesAsync();
    }
} 