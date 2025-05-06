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
                var activeMembers = groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);
                
                foreach (var member in activeMembers)
                {
                    if (member.UserId.HasValue)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Conference Registration",
                            Message = $"Project '{project.ProjectName}' has been registered for conference '{request.ConferenceName}'",
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

            var groupMembers = await _groupRepository.GetMembersByGroupId(conference.Project.GroupId.Value);
            foreach (var member in groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    await _notificationService.CreateNotification(new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "Conference đã được phê duyệt",
                        Message = $"Conference '{conference.ConferenceName}' đã được hội đồng phê duyệt",
                        ProjectId = conference.ProjectId.Value,
                        Status = 0,
                        IsRead = false
                    });
                }
            }
            // Gửi email cho stakeholder
            var stakeholders = groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            foreach (var stakeholder in stakeholders)
            {
                await _emailService.SendEmailAsync(
                stakeholder.User.Email,
                $"Conference đã được phê duyệt",
                $"Xin chào {stakeholder.User.FullName},\n\n" +
                $"Conference '{conference.ConferenceName}' đã được hội đồng phê duyệt.\n\n" +
                $"Trân trọng."
                );
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

            var groupMembers = await _groupRepository.GetMembersByGroupId(conference.Project.GroupId.Value);
            foreach (var member in groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    await _notificationService.CreateNotification(new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "Conference đã bị từ chối",
                        Message = $"Conference '{conference.ConferenceName}' đã bị hội đồng từ chối. Vui lòng xem biên bản tại: {documentUrl}",
                        ProjectId = conference.ProjectId.Value,
                        Status = 0,
                        IsRead = false
                    });
                }
            }
            // Gửi email cho stakeholder
            var stakeholders = groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            foreach (var stakeholder in stakeholders)
            {
                await _emailService.SendEmailAsync(
                stakeholder.User.Email,
                $"Conference đã bị từ chối",
                $"Xin chào {stakeholder.User.FullName},\n\n" +
                $"Conference '{conference.ConferenceName}' đã bị hội đồng từ chối.\n" +
                $"Vui lòng xem biên bản tại: {documentUrl}\n\n" +
                $"Trân trọng."
                );
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
                
                // Expense information
                Expense = conference.ConferenceExpenses.Any() 
                    ? _mapper.Map<ConferenceExpenseResponse>(conference.ConferenceExpenses.First()) 
                    : null,
                
                // Documents
                Documents = _mapper.Map<List<DocumentResponse>>(documents)
            };
            
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

    public async Task<int> RequestConferenceExpenseAsync(int userId, RequestConferenceExpenseRequest request)
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
            
            // Create expense request
            var expenseRequest = new ProjectRequest
            {
                ProjectId = conference.ProjectId ?? 0,
                RequestType = ProjectRequestTypeEnum.Conference_Expense,
                RequestedById = userId,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatusEnum.Pending
            };
            
            await _context.ProjectRequests.AddAsync(expenseRequest);
            await _context.SaveChangesAsync();
            
            // Create conference expense
            var expense = new ConferenceExpense
            {
                ConferenceId = conference.ConferenceId,
                Accomodation = request.Accommodation,
                AccomodationExpense = request.AccommodationExpense,
                Travel = request.Travel,
                TravelExpense = request.TravelExpense
            };
            
            await _context.ConferenceExpenses.AddAsync(expense);
            await _context.SaveChangesAsync();
            
            // Associate expense with request
            var fundDisbursement = new FundDisbursement
            {
                FundRequest = request.AccommodationExpense + request.TravelExpense,
                ProjectId = conference.ProjectId ?? 0,
                UserRequest = userId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)FundDisbursementStatusEnum.Pending,
                Description = $"Conference expense request for {conference.ConferenceName}"
            };
            
            await _context.FundDisbursements.AddAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Update project request with fund disbursement ID
            expenseRequest.FundDisbursementId = fundDisbursement.FundDisbursementId;
            _context.ProjectRequests.Update(expenseRequest);
            await _context.SaveChangesAsync();
            
            // Notify office users instead of department staff
            var officeUsers = await _context.Users
                .Where(u => u.Role == (int)SystemRoleEnum.Office || u.Role == (int)SystemRoleEnum.Accounting_Department)
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
} 