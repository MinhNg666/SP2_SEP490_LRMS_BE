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
            // Kiểm tra project có tồn tại không
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException("Không tìm thấy project");

            // Kiểm tra có phải là Research project không
            if (project.ProjectType != (int)ProjectTypeEnum.Research)
                throw new ServiceException("Chỉ có thể tạo Journal từ Research project");

            // Kiểm tra người yêu cầu có phải là leader không
            var leaderMember = project.Group.GroupMembers
                .FirstOrDefault(m => m.UserId == userId && m.Status == (int)GroupMemberStatus.Active);

            if (leaderMember == null || leaderMember.Role != (int)GroupMemberRoleEnum.Leader)
                throw new ServiceException("Chỉ leader mới có quyền tạo Journal");

            // Kiểm tra tất cả milestone đã hoàn thành chưa
            if (project.ProjectPhases.Any(m => m.Status != (int)ProjectPhaseStatusEnum.Completed))
                throw new ServiceException("Tất cả milestone phải hoàn thành trước khi tạo Journal");

            // Cập nhật project hiện có
            project.ProjectType = (int)ProjectTypeEnum.Journal;
            project.Status = (int)ProjectStatusEnum.Pending;
            await _context.SaveChangesAsync();

            // Tạo Journal record
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
                var emailSubject = $"[LRMS] Notification: Project Converted to Journal - {project.ProjectName}";
                var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                    ? JournalEmailTemplates.GetStakeholderJournalCreationEmail(member.User, project, journal, group, creator)
                    : JournalEmailTemplates.GetMemberJournalCreationEmail(member.User, project, journal, group, creator);

                await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                // Gửi notification cho thành viên thường
                if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "New Journal Publication Registered", // Đã thay đổi
                        Message = $"A new journal article '{journal.JournalName}' has been registered for project '{project.ProjectName}'", // Đã thay đổi
                        ProjectId = project.ProjectId,
                        Status = 0,
                        IsRead = false
                    };
                    await _notificationService.CreateNotification(notificationRequest);
                }
            }
        }        
        return projectId;
    }
    catch (Exception ex)
    {
        throw new ServiceException($"Lỗi khi tạo Journal: {ex.Message}");
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
                throw new ServiceException("Không tìm thấy journal");

            // Kiểm tra người phê duyệt có phải là thư ký hội đồng không
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền phê duyệt journal");

            // Kiểm tra thư ký có cùng department với project không
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != journal.Project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với journal này");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/council-documents");

            // Tạo ProjectResource cho document
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

            // Tạo Document record
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

            // Cập nhật trạng thái project và journal
            journal.Project.Status = (int)ProjectStatusEnum.Approved;
            journal.PublisherStatus = (int)PublisherStatusEnum.Approved;
            await _context.SaveChangesAsync();

            // Lấy thông tin approver
            var approver = await _context.Users.FindAsync(secretaryId);
            var group = journal.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: Journal Approved - {journal.JournalName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? JournalEmailTemplates.GetStakeholderJournalApprovalEmail(member.User, journal.Project, journal, group, approver, documentUrl)
                        : JournalEmailTemplates.GetMemberJournalApprovalEmail(member.User, journal.Project, journal, group, approver, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Journal Approved", // Đã thay đổi
                            Message = $"Journal '{journal.JournalName}' has been approved. Please check the council meeting documents for details.", // Đã thay đổi
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
            throw new ServiceException($"Lỗi khi phê duyệt journal: {ex.Message}");
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
                throw new ServiceException("Không tìm thấy journal");

            // Kiểm tra người từ chối có phải là thư ký hội đồng không
            var secretary = await _groupRepository.GetMemberByUserId(secretaryId);
            if (secretary == null || secretary.Role != (int)GroupMemberRoleEnum.Secretary)
                throw new ServiceException("Bạn không có quyền từ chối journal");

            // Kiểm tra thư ký có cùng department với project không
            var secretaryGroup = await _groupRepository.GetByIdAsync(secretary.GroupId.Value);
            if (secretaryGroup.GroupDepartment != journal.Project.DepartmentId)
                throw new ServiceException("Bạn không thuộc cùng phòng ban với journal này");

            // Upload document
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/council-documents");

            // Tạo ProjectResource cho document
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

            // Tạo Document record
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

            // Cập nhật trạng thái project và journal
            journal.Project.Status = (int)ProjectStatusEnum.Rejected;
            journal.PublisherStatus = (int)PublisherStatusEnum.Rejected;
            await _context.SaveChangesAsync();

            // Lấy thông tin
            var group = journal.Project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Notification: Journal Rejected - {journal.JournalName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? JournalEmailTemplates.GetStakeholderJournalRejectionEmail(member.User, journal.Project, journal, group, documentUrl)
                        : JournalEmailTemplates.GetMemberJournalRejectionEmail(member.User, journal.Project, journal, group, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = "Journal Rejected", // Đã thay đổi
                            Message = $"Journal '{journal.JournalName}' has been rejected. Please check the council meeting documents for details.", // Đã thay đổi
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
            throw new ServiceException($"Lỗi khi từ chối journal: {ex.Message}");
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
                throw new ServiceException("Không tìm thấy journal");

            if (documentFile == null)
                throw new ServiceException("Vui lòng cung cấp file tài liệu");

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
            throw new ServiceException($"Lỗi khi thêm tài liệu cho journal: {ex.Message}");
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
                        ProjectResourceId = projectResource.ProjectResourceId
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
} 