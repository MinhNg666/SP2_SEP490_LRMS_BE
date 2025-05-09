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

namespace Service.Implementations;

public class JournalService : IJournalService
{
    private readonly IJournalRepository _journalRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IS3Service _s3Service;
    private readonly ITimelineService _timelineService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public JournalService(
        IJournalRepository journalRepository,
        IProjectRepository projectRepository,
        IGroupRepository groupRepository,
        INotificationService notificationService,
        IS3Service s3Service,
        ITimelineService timelineService,
        IMapper mapper,
        LRMSDbContext context,
        IEmailService emailService)
    {
        _journalRepository = journalRepository;
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
        _s3Service = s3Service;
        _timelineService = timelineService;
        _mapper = mapper;
        _context = context;
        _emailService = emailService;
    }

    public async Task<JournalResponse> CreateJournal(CreateJournalRequest request, int createdBy)
    {
        try
        {
            // Kiểm tra project tồn tại
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");

            // Tạo journal
            var journal = new Journal
            {
                JournalName = request.JournalName,
                PublisherName = request.PublisherName,
                PublisherStatus = request.PublisherStatus,
                DoiNumber = request.DoiNumber,
                AcceptanceDate = request.AcceptanceDate,
                PublicationDate = request.PublicationDate,
                SubmissionDate = request.SubmissionDate,
                ReviewerComments = request.ReviewerComments,
                ProjectId = request.ProjectId
            };

            await _journalRepository.AddAsync(journal);
            
            // Lấy thông tin journal vừa tạo kèm theo project
            var createdJournal = await _journalRepository.GetJournalWithDetailsAsync(journal.JournalId);
            return _mapper.Map<JournalResponse>(createdJournal);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating journal: {ex.Message}");
        }
    }

    public async Task<int> CreateJournalFromResearch(int projectId, int userId, CreateJournalFromProjectRequest request)
    {
        try
        {
            // Check if project exists
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException("Project not found");

            // Check if project status is approved
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Only approved projects can register for journals");

            // Check if user is in the project group
            var isUserInGroup = project.Group?.GroupMembers
                .Any(gm => gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active) ?? false;

            if (!isUserInGroup)
                throw new ServiceException("You must be a member of the project group to register for a journal");

            // Create Journal record with minimal information (don't change project type or status)
            var journal = new Journal
            {
                JournalName = request.JournalName,
                PublisherName = request.PublisherName,
                ProjectId = projectId,
                PublisherStatus = (int)PublisherStatusEnum.Pending,
                SubmissionDate = DateTime.Now
            };

            await _context.Journals.AddAsync(journal);
            await _context.SaveChangesAsync();

            // Send notification to members
            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            var activeMembers = groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Create journal details
            var journalInfo = new StringBuilder();
            journalInfo.AppendLine("Journal Details:");
            journalInfo.AppendLine($"- Journal Name: {journal.JournalName}");
            journalInfo.AppendLine($"- Publisher Name: {journal.PublisherName}");
            journalInfo.AppendLine($"- Submission Date: {journal.SubmissionDate:dd/MM/yyyy}");

            // Send notification to regular members
            foreach (var member in activeMembers.Where(m => m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "New Journal Publication Registration",
                        Message = $"A new journal submission has been registered for '{journal.JournalName}'.\n\n{journalInfo}",
                        ProjectId = projectId,
                        Status = 0,
                        IsRead = false
                    };
                    await _notificationService.CreateNotification(notificationRequest);
                }
            }
            // Send email to stakeholders
            var stakeholders = activeMembers.Where(m =>
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            foreach (var stakeholder in stakeholders)
            {
                await _emailService.SendEmailAsync(
                stakeholder.User.Email,
                "New Journal Publication Registration",
                $"Hello {stakeholder.User.FullName},\n\n" +
                $"A new journal submission has been registered for '{journal.JournalName}'.\n\n" +
                $"{journalInfo}\n\n" +
                "Best regards."
                );
            }

            return journal.JournalId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating journal: {ex.Message}");
        }
    }

    public async Task<bool> ApproveJournal(int journalId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                throw new ServiceException("Journal not found");

            // Check if approver is secretary of council
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("You don't have permission to approve this journal");

            // Check if secretary is in the same department as the project
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != journal.Project.DepartmentId)
                throw new ServiceException("You are not in the same department as this journal");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/council-documents");

            // Create ProjectResource for document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = journal.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            // Create Document record
            var document = new Document
            {
                ProjectId = journal.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = projectResource.ProjectResourceId
            };
            await _context.Documents.AddAsync(document);

            // Update project and journal status
            journal.Project.Status = (int)ProjectStatusEnum.Approved;
            journal.PublisherStatus = (int)PublisherStatusEnum.Approved;
            await _context.SaveChangesAsync();

            // Send notification to members
            var groupMembers = await _groupRepository.GetMembersByGroupId(journal.Project.GroupId.Value);
            foreach (var member in groupMembers.Where(m =>
                m.Status == (int)GroupMemberStatus.Active &&
                m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    await _notificationService.CreateNotification(new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "Journal Approved",
                        Message = $"Journal '{journal.JournalName}' has been approved by the council",
                        ProjectId = journal.ProjectId.Value,
                        Status = 0,
                        IsRead = false
                    });
                }
            }
            // Send email to stakeholders
            var stakeholders = groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            foreach (var stakeholder in stakeholders)
            {
                await _emailService.SendEmailAsync(
                    stakeholder.User.Email,
                    "Journal Approved",
                    $"Hello {stakeholder.User.FullName},\n\n" +
                    $"Journal '{journal.JournalName}' has been approved by the council.\n\n" +
                    "Best regards."
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error approving journal: {ex.Message}");
        }
    }

    public async Task<bool> RejectJournal(int journalId, int secretaryId, IFormFile documentFile)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                throw new ServiceException("Journal not found");

            // Check if rejector is secretary of council
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("You don't have permission to reject this journal");

            // Check if secretary is in the same department as the project
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != journal.Project.DepartmentId)
                throw new ServiceException("You are not in the same department as this journal");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/council-documents");

            // Create ProjectResource for document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = journal.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            // Create Document record
            var document = new Document
            {
                ProjectId = journal.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.CouncilDecision,
                UploadAt = DateTime.Now,
                UploadedBy = secretaryId,
                ProjectResourceId = projectResource.ProjectResourceId
            };
            await _context.Documents.AddAsync(document);

            // Update project and journal status
            journal.Project.Status = (int)ProjectStatusEnum.Rejected;
            journal.PublisherStatus = (int)PublisherStatusEnum.Rejected;
            await _context.SaveChangesAsync();

            // Send notification to members
            var groupMembers = await _groupRepository.GetMembersByGroupId(journal.Project.GroupId.Value);
            foreach (var member in groupMembers.Where(m =>
                m.Status == (int)GroupMemberStatus.Active &&
                m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    await _notificationService.CreateNotification(new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "Journal Rejected",
                        Message = $"Journal '{journal.JournalName}' has been rejected by the council. Please see the document at: {documentUrl}",
                        ProjectId = journal.ProjectId.Value,
                        Status = 0,
                        IsRead = false
                    });
                }
            }
            // Send email to stakeholders
            var stakeholders = groupMembers.Where(m =>
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role == (int)GroupMemberRoleEnum.Stakeholder &&
            m.User != null);

            foreach (var stakeholder in stakeholders)
            {
                await _emailService.SendEmailAsync(
                stakeholder.User.Email,
                "Journal Rejected",
                $"Hello {stakeholder.User.FullName},\n\n" +
                $"Journal '{journal.JournalName}' has been rejected by the council.\n" +
                $"Please see the document at: {documentUrl}\n\n" +
                "Best regards."
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error rejecting journal: {ex.Message}");
        }
    }

    public async Task<bool> AddJournalDocument(int journalId, int userId, IFormFile documentFile)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                throw new ServiceException("Journal not found");

            if (documentFile == null)
                throw new ServiceException("Please provide a document file");

            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/documents");

            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1,
                ProjectId = journal.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };

            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            var document = new Document
            {
                ProjectId = journal.ProjectId.Value,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.JournalPaper,
                UploadAt = DateTime.Now,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            // Lấy thông tin cần thiết
            var uploader = await _context.Users.FindAsync(userId);
            var group = journal.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: New Document for Journal - {journal.JournalName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? JournalEmailTemplates.GetStakeholderJournalDocumentEmail(member.User, journal.Project, journal, uploader, documentFile.FileName, documentUrl)
                        : JournalEmailTemplates.GetMemberJournalDocumentEmail(member.User, journal.Project, journal, uploader, documentFile.FileName, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        string title = member.UserId.Value == userId
                            ? "You Have Uploaded New Document" // Đã thay đổi
                            : "New Document in Journal"; // Đã thay đổi

                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = title,
                            Message = $"Project: {journal.JournalName}\nDocument: {documentFile.FileName}\nUploaded by: {uploader?.FullName}", // Đã thay đổi
                            ProjectId = journal.ProjectId.Value,
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
            throw new ServiceException($"Error adding document to journal: {ex.Message}");
        }
    }

    public async Task<IEnumerable<JournalResponse>> GetAllJournals()
    {
        try
        {
            var journals = await _journalRepository.GetAllJournalsWithDetailsAsync();
            Console.WriteLine($"Found {journals?.Count() ?? 0} journals");

            if (journals == null || !journals.Any())
            {
                Console.WriteLine("No journals found");
                return new List<JournalResponse>();
            }

            var responses = _mapper.Map<IEnumerable<JournalResponse>>(journals);
            Console.WriteLine($"Mapped {responses?.Count() ?? 0} journal responses");

            return responses ?? new List<JournalResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllJournals: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new ServiceException($"Error getting all journals: {ex.Message}");
        }
    }

    public async Task<JournalResponse> GetJournalById(int journalId)
    {
        try
        {
            var journal = await _journalRepository.GetJournalWithDetailsAsync(journalId);
            if (journal == null)
                throw new ServiceException("Journal not found");

            return _mapper.Map<JournalResponse>(journal);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting journal by id: {ex.Message}");
        }
    }

    public async Task<IEnumerable<JournalResponse>> GetJournalsByProjectId(int projectId)
    {
        try
        {
            var journals = await _journalRepository.GetAllJournalsWithDetailsAsync();
            var projectJournals = journals.Where(j => j.ProjectId == projectId);
            return _mapper.Map<IEnumerable<JournalResponse>>(projectJournals);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting journals by project id: {ex.Message}");
        }
    }

    public async Task AddJournalDocuments(int journalId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);
            
            if (journal == null)
                throw new ServiceException("Journal not found");
            
            int projectId = journal.ProjectId.Value;
            
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/journals/{journalId}");
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
                        DocumentType = (int)DocumentTypeEnum.JournalPaper, 
                        UploadAt = DateTime.Now,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        JournalId = journalId
                    };

                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading journal documents: {ex.Message}");
        }
    }

    public async Task<IEnumerable<JournalResponse>> GetUserJournals(int userId)
    {
        try
        {
            // Get all groups where the user is a member
            var userGroups = await _groupRepository.GetGroupsByUserId(userId);
            if (!userGroups.Any())
                return Enumerable.Empty<JournalResponse>();
            
            var groupIds = userGroups.Select(g => g.GroupId).ToList();
            
            // Get all projects for these groups
            var projects = await _context.Projects
                .Where(p => p.GroupId.HasValue && groupIds.Contains(p.GroupId.Value))
                .Select(p => p.ProjectId)
                .ToListAsync();
            
            if (!projects.Any())
                return Enumerable.Empty<JournalResponse>();
            
            // Get all journals for these projects
            var journals = await _context.Journals
                .Include(j => j.Project)
                .Where(j => j.ProjectId.HasValue && projects.Contains(j.ProjectId.Value))
                .ToListAsync();
            
            // Map to response objects
            var responses = _mapper.Map<IEnumerable<JournalResponse>>(journals).ToList();
            
            foreach (var response in responses)
            {
                var journal = journals.FirstOrDefault(j => j.JournalId == response.JournalId);
                if (journal != null && journal.Project != null)
                {
                    response.ProjectName = journal.Project.ProjectName;
                }
                
                // Add publisher status name
                if (journal?.PublisherStatus.HasValue == true)
                {
                    response.PublisherStatusName = Enum.GetName(typeof(PublisherStatusEnum), journal.PublisherStatus.Value) ?? "Unknown";
                }
            }
            
            return responses;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving user journals: {ex.Message}");
        }
    }

    public async Task<JournalDetailResponse> GetJournalDetails(int journalId)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);
            
            if (journal == null)
                throw new ServiceException($"Journal with ID {journalId} not found");
            
            // Get documents related to this journal, preferring JournalId over ProjectId+DocumentType
            var documents = await _context.Documents
                .Where(d => d.JournalId == journalId || 
                      (d.ProjectId == journal.ProjectId && 
                       d.DocumentType == (int)DocumentTypeEnum.JournalPaper))
                .ToListAsync();
            
            var response = new JournalDetailResponse
            {
                JournalId = journal.JournalId,
                JournalName = journal.JournalName,
                PublisherName = journal.PublisherName,
                PublisherStatus = journal.PublisherStatus ?? 0,
                PublisherStatusName = journal.PublisherStatus.HasValue ? 
                    Enum.GetName(typeof(PublisherStatusEnum), journal.PublisherStatus.Value) : "Unknown",
                DoiNumber = journal.DoiNumber,
                AcceptanceDate = journal.AcceptanceDate,
                PublicationDate = journal.PublicationDate,
                SubmissionDate = journal.SubmissionDate,
                ReviewerComments = journal.ReviewerComments,
                JournalFunding = journal.JournalFunding,
                JournalStatus = journal.JournalStatus,
                
                // Project information
                ProjectId = journal.ProjectId ?? 0,
                ProjectName = journal.Project?.ProjectName ?? "Unknown",
                ProjectStatus = journal.Project?.Status ?? 0,
                
                // Documents
                Documents = _mapper.Map<List<DocumentResponse>>(documents)
            };
            
            return response;
        }
        catch (Exception ex) when (ex is not ServiceException)
        {
            throw new ServiceException($"Error retrieving journal details: {ex.Message}");
        }
    }

    public async Task<bool> UpdateJournalStatus(int journalId, UpdateJournalStatusRequest request, int userId)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                throw new ServiceException("Journal not found");

            // Verify user has permission to update (either a group member or a lecturer)
            var isUserInGroup = journal.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;

            // Get user role
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;

            if (!isUserInGroup && !isLecturer)
                throw new ServiceException("You don't have permission to update this journal's status");

            // Update journal status
            journal.PublisherStatus = request.PublisherStatus;
            journal.ReviewerComments = request.ReviewerComments;

            _context.Journals.Update(journal);
            await _context.SaveChangesAsync();

            // Send notification to group members
            if (journal.Project?.GroupId.HasValue == true)
            {
                var groupMembers = await _groupRepository.GetMembersByGroupId(journal.Project.GroupId.Value);
                
                // Get status name for the notification
                string statusName = Enum.GetName(typeof(PublisherStatusEnum), request.PublisherStatus) ?? request.PublisherStatus.ToString();
                
                foreach (var member in groupMembers.Where(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.UserId != userId)) // Don't notify the user who made the update
                {
                    if (member.UserId.HasValue)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = $"Journal Status Updated",
                            Message = $"The status for journal '{journal.JournalName}' has been updated to {statusName}",
                            ProjectId = journal.ProjectId ?? 0,
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
            throw new ServiceException($"Error updating journal status: {ex.Message}");
        }
    }

    public async Task<bool> UpdateJournalDetails(int journalId, UpdateJournalDetailsRequest request, int userId)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);

            if (journal == null)
                throw new ServiceException("Journal not found");

            // Check if publisher status is approved
            if (journal.PublisherStatus != (int)PublisherStatusEnum.Approved)
                throw new ServiceException("Journal details can only be updated when the publisher status is approved");

            // Verify user has permission to update (either a group member or a lecturer)
            var isUserInGroup = journal.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;

            // Get user role
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            bool isLecturer = user?.Role == (int)SystemRoleEnum.Lecturer;

            if (!isUserInGroup && !isLecturer)
                throw new ServiceException("You don't have permission to update this journal's details");

            // Update journal details
            journal.DoiNumber = request.DoiNumber;
            journal.AcceptanceDate = request.AcceptanceDate;
            journal.PublicationDate = request.PublicationDate;
            journal.JournalFunding = request.JournalFunding;

            _context.Journals.Update(journal);
            await _context.SaveChangesAsync();

            // Send notification to group members
            if (journal.Project?.GroupId.HasValue == true)
            {
                var groupMembers = await _groupRepository.GetMembersByGroupId(journal.Project.GroupId.Value);
                
                foreach (var member in groupMembers.Where(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.UserId != userId)) // Don't notify the user who made the update
                {
                    if (member.UserId.HasValue)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = $"Journal Details Updated",
                            Message = $"The details for journal '{journal.JournalName}' have been updated",
                            ProjectId = journal.ProjectId ?? 0,
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
            throw new ServiceException($"Error updating journal details: {ex.Message}");
        }
    }

    public async Task<int> RequestJournalFundingAsync(int userId, RequestJournalFundingRequest request, IEnumerable<IFormFile> documentFiles)
    {
        try
        {
            // Get the journal and verify it exists
            var journal = await _context.Journals
                .Include(j => j.Project)
                    .ThenInclude(p => p.Group)
                        .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(j => j.JournalId == request.JournalId);
            
            if (journal == null)
                throw new ServiceException("Journal not found");
            
            // Verify journal is approved
            if (journal.PublisherStatus != (int)PublisherStatusEnum.Approved)
                throw new ServiceException("Cannot request funding - journal must have approved publisher status");
            
            // Verify user is authorized (member of the project group)
            bool isUserAuthorized = journal.Project?.Group?.GroupMembers
                .Any(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active) ?? false;
            
            if (!isUserAuthorized)
                throw new ServiceException("You are not authorized to request funding for this journal");
            
            // Check for existing journal funding requests
            var existingFundDisbursements = await _context.FundDisbursements
                .Where(fd => fd.JournalId == journal.JournalId && 
                             fd.FundDisbursementType == (int)FundDisbursementTypeEnum.JournalFunding)
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
                    throw new ServiceException("A funding request for this journal has already been approved. No additional funding requests can be made.");
                
                // Check for pending requests - if any are pending, no new requests allowed
                bool hasPending = existingRequests.Any(pr => pr.ApprovalStatus == ApprovalStatusEnum.Pending);
                if (hasPending)
                    throw new ServiceException("There is already a pending funding request for this journal. Please wait for it to be processed.");
                
                // If we're here, all previous requests were rejected, so we can continue
            }
            
            // Update journal details
            journal.DoiNumber = request.DoiNumber;
            journal.AcceptanceDate = request.AcceptanceDate;
            journal.PublicationDate = request.PublicationDate;
            journal.JournalFunding = request.JournalFunding;
            
            _context.Journals.Update(journal);
            await _context.SaveChangesAsync();
            
            // Create fund disbursement with type JournalFunding
            var fundDisbursement = new FundDisbursement
            {
                FundRequest = request.JournalFunding,
                ProjectId = journal.ProjectId ?? 0,
                UserRequest = userId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)FundDisbursementStatusEnum.Pending,
                Description = $"Journal funding request for {journal.JournalName}",
                JournalId = journal.JournalId,
                FundDisbursementType = (int)FundDisbursementTypeEnum.JournalFunding
            };
            
            await _context.FundDisbursements.AddAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Create project request with type Fund_Disbursement
            var fundingRequest = new ProjectRequest
            {
                ProjectId = journal.ProjectId ?? 0,
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
                string folderPath = $"projects/{journal.ProjectId}/journals/{journal.JournalId}/funding";
                var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create resource
                    var projectResource = new ProjectResource
                    {
                        ResourceName = file.FileName,
                        ResourceType = 1, // Document
                        ProjectId = journal.ProjectId ?? 0,
                        Acquired = true,
                        Quantity = 1
                    };
                    
                    await _context.ProjectResources.AddAsync(projectResource);
                    await _context.SaveChangesAsync();
                    
                    // Create document
                    var document = new Document
                    {
                        ProjectId = journal.ProjectId ?? 0,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.JournalFunding,
                        UploadAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId,
                        FundDisbursementId = fundDisbursement.FundDisbursementId,
                        RequestId = fundingRequest.RequestId,
                        JournalId = journal.JournalId
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
                    Title = "New Journal Funding Request",
                    Message = $"A new journal funding request has been submitted for '{journal.JournalName}' (Amount: {fundDisbursement.FundRequest:C})",
                    ProjectId = journal.ProjectId.Value,
                    Status = 0,
                    IsRead = false
                };
                
                await _notificationService.CreateNotification(notification);
            }
            
            return fundingRequest.RequestId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error requesting journal funding: {ex.Message}");
        }
    }

    public async Task<bool> ApproveJournalFundingRequest(int requestId, int approverId, IEnumerable<IFormFile> documentFiles)
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
            
            // Check if this is a journal funding request
            if (!fundDisbursement.JournalId.HasValue)
                throw new ServiceException("The fund disbursement is not for a journal");
            
            if (fundDisbursement.FundDisbursementType != (int)FundDisbursementTypeEnum.JournalFunding)
                throw new ServiceException("The fund disbursement is not for journal funding");
            
            // Get the journal
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.JournalId == fundDisbursement.JournalId.Value);
            
            if (journal == null)
                throw new ServiceException("Journal not found");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Update fund disbursement status
                fundDisbursement.Status = (int)FundDisbursementStatusEnum.Approved;
                
                _context.FundDisbursements.Update(fundDisbursement);
                
                // Find or create active quota for this project
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
                
                // Update project request status
                request.ApprovalStatus = ApprovalStatusEnum.Approved;
                request.ApprovedById = approverId;
                request.ApprovedAt = DateTime.UtcNow;
                _context.ProjectRequests.Update(request);
                
                // Upload documents if provided
                if (documentFiles != null && documentFiles.Any())
                {
                    string folderPath = $"projects/{fundDisbursement.ProjectId}/journals/{journal.JournalId}/funding/approval";
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
                            DocumentType = (int)DocumentTypeEnum.FundingConfirmation,
                            UploadAt = DateTime.UtcNow,
                            UploadedBy = approverId,
                            ProjectResourceId = projectResource.ProjectResourceId,
                            FundDisbursementId = fundDisbursement.FundDisbursementId,
                            RequestId = requestId,
                            JournalId = journal.JournalId
                        };
                        
                        await _context.Documents.AddAsync(document);
                        index++;
                    }
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                // Notify the requester about the approval
                if (request.RequestedBy != null)
                {
                    var notification = new CreateNotificationRequest
                    {
                        UserId = request.RequestedById,
                        Title = "Journal Funding Request Approved",
                        Message = $"Your funding request for journal '{journal.JournalName}' has been approved (Amount: {fundDisbursement.FundRequest:C})",
                        ProjectId = request.ProjectId,
                        Status = 0,
                        IsRead = false
                    };
                    
                    await _notificationService.CreateNotification(notification);
                    
                    // Also send email notification if possible
                    if (_emailService != null && !string.IsNullOrEmpty(request.RequestedBy.Email))
                    {
                        string emailBody = $"Dear {request.RequestedBy.FullName},\n\n" +
                                          $"Your funding request for journal '{journal.JournalName}' has been approved.\n\n" +
                                         $"Amount: {fundDisbursement.FundRequest:C}\n\n" +
                                         $"This is an automated message. Please do not reply to this email.";
                                         
                        await _emailService.SendEmailAsync(
                            request.RequestedBy.Email,
                            "Journal Funding Request Approved",
                            emailBody
                        );
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ServiceException($"Error during approval transaction: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error approving journal funding request: {ex.Message}");
        }
    }

    public async Task<bool> RejectJournalFundingRequest(int requestId, int rejecterId, string rejectionReason, IEnumerable<IFormFile> documentFiles)
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
            
            // Check if this is a journal funding request
            if (!fundDisbursement.JournalId.HasValue)
                throw new ServiceException("The fund disbursement is not for a journal");
            
            if (fundDisbursement.FundDisbursementType != (int)FundDisbursementTypeEnum.JournalFunding)
                throw new ServiceException("The fund disbursement is not for journal funding");
            
            // Get the journal
            var journal = await _context.Journals
                .FirstOrDefaultAsync(j => j.JournalId == fundDisbursement.JournalId.Value);
            
            if (journal == null)
                throw new ServiceException("Journal not found");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Update fund disbursement status
                fundDisbursement.Status = (int)FundDisbursementStatusEnum.Rejected;
                fundDisbursement.RejectionReason = rejectionReason;
                
                _context.FundDisbursements.Update(fundDisbursement);
                
                // Update project request status
                request.ApprovalStatus = ApprovalStatusEnum.Rejected;
                request.ApprovedById = rejecterId;
                request.ApprovedAt = DateTime.UtcNow;
                request.RejectionReason = rejectionReason;
                
                _context.ProjectRequests.Update(request);
                
                // Upload documents if provided
                if (documentFiles != null && documentFiles.Any())
                {
                    string folderPath = $"projects/{fundDisbursement.ProjectId}/journals/{journal.JournalId}/funding/rejection";
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
                            DocumentType = (int)DocumentTypeEnum.FundingConfirmation,
                            UploadAt = DateTime.UtcNow,
                            UploadedBy = rejecterId,
                            ProjectResourceId = projectResource.ProjectResourceId,
                            FundDisbursementId = fundDisbursement.FundDisbursementId,
                            RequestId = requestId,
                            JournalId = journal.JournalId
                        };
                        
                        await _context.Documents.AddAsync(document);
                        index++;
                    }
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                // Notify the requester about the rejection
                if (request.RequestedBy != null)
                {
                    var notification = new CreateNotificationRequest
                    {
                        UserId = request.RequestedById,
                        Title = "Journal Funding Request Rejected",
                        Message = $"Your funding request for journal '{journal.JournalName}' has been rejected. Reason: {rejectionReason}",
                        ProjectId = request.ProjectId,
                        Status = 0,
                        IsRead = false
                    };
                    
                    await _notificationService.CreateNotification(notification);
                    
                    // Also send email notification if possible
                    if (_emailService != null && !string.IsNullOrEmpty(request.RequestedBy.Email))
                    {
                        string emailBody = $"Dear {request.RequestedBy.FullName},\n\n" +
                                          $"Your funding request for journal '{journal.JournalName}' has been rejected.\n\n" +
                                         $"Reason: {rejectionReason}\n\n" +
                                         $"Amount Requested: {fundDisbursement.FundRequest:C}\n\n" +
                                         $"This is an automated message. Please do not reply to this email.";
                                         
                        await _emailService.SendEmailAsync(
                            request.RequestedBy.Email,
                            "Journal Funding Request Rejected",
                            emailBody
                        );
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ServiceException($"Error during rejection transaction: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error rejecting journal funding request: {ex.Message}");
        }
    }
} 