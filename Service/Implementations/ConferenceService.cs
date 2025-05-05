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

    public ConferenceService(
        IConferenceRepository conferenceRepository,
        IProjectRepository projectRepository,
        IGroupRepository groupRepository,
        INotificationService notificationService,
        IS3Service s3Service,
        ITimelineService timelineService,
        IMapper mapper,
        LRMSDbContext context,
        IEmailService emailService)
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
    public async Task<int> CreateConferenceFromResearch(int projectId, int leaderId, CreateConferenceFromProjectRequest request, IFormFile documentFile)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectPhases)
                .Include(p => p.Group)
                    .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ServiceException("Không tìm thấy project");

            if (project.ProjectType != (int)ProjectTypeEnum.Research)
                throw new ServiceException("Chỉ có thể tạo Conference từ Research project");

            var leaderMember = project.Group.GroupMembers
                .FirstOrDefault(m => m.UserId == leaderId && m.Status == (int)GroupMemberStatus.Active);

            if (leaderMember == null || leaderMember.Role != (int)GroupMemberRoleEnum.Leader)
                throw new ServiceException("Chỉ leader mới có quyền tạo Conference");

            if (project.ProjectPhases.Any(m => m.Status != (int)ProjectPhaseStatusEnum.Completed))
                throw new ServiceException("Tất cả milestone phải hoàn thành trước khi tạo Conference");

            // Cập nhật project hiện có
            project.ProjectType = (int)ProjectTypeEnum.Conference;
            project.Status = (int)ProjectStatusEnum.Pending;
            await _context.SaveChangesAsync();

            var conference = new Conference
            {
                ConferenceName = request.ConferenceName,
                Location = request.Location,
                ConferenceRanking = request.ConferenceRanking,
                PresentationDate = request.PresentationDate,
                PresentationType = request.PresentationType,
                ProjectId = projectId
            };

            await _context.Conferences.AddAsync(conference);

            if (documentFile != null)
            {
                var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"conferences/{conference.ConferenceId}/documents");

                var projectResource = new ProjectResource
                {
                    ResourceName = documentFile.FileName,
                    ResourceType = 1,
                    ProjectId = projectId,
                    Acquired = true,
                    Quantity = 1
                };

                await _context.ProjectResources.AddAsync(projectResource);
                await _context.SaveChangesAsync();

                var document = new Document
                {
                    ProjectId = projectId,
                    DocumentUrl = documentUrl,
                    FileName = documentFile.FileName,
                    DocumentType = (int)DocumentTypeEnum.ConferenceProposal,
                    UploadAt = DateTime.Now,
                    UploadedBy = leaderId,
                    ProjectResourceId = projectResource.ProjectResourceId
                };

                await _context.Documents.AddAsync(document);
            }

            await _context.SaveChangesAsync();

            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            // Lấy thông tin cần thiết
            var creator = await _context.Users.FindAsync(leaderId);
            var group = project.Group;
            var activeMembers = group.GroupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active);

            // Gửi email và notification cho từng thành viên
            foreach (var member in activeMembers)
            {
                if (member.UserId.HasValue && member.User != null)
                {
                    // Gửi email
                    var emailSubject = $"[LRMS] Thông báo: Dự án chuyển đổi thành Conference - {project.ProjectName}";
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
                            Title = "Đăng ký tham gia Conference mới",
                            Message = $"Dự án '{project.ProjectName}' đã được chuyển thành Conference '{conference.ConferenceName}' và đang chờ phê duyệt",
                            ProjectId = projectId,
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
            throw new ServiceException($"Lỗi khi tạo Conference: {ex.Message}");
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
                    var emailSubject = $"[LRMS] Thông báo: Conference đã được phê duyệt - {conference.ConferenceName}";
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
                            Title = "Conference đã được phê duyệt",
                            Message = $"Conference '{conference.ConferenceName}' đã được hội đồng phê duyệt",
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
                    var emailSubject = $"[LRMS] Thông báo: Conference đã bị từ chối - {conference.ConferenceName}";
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
                            Title = "Conference đã bị từ chối",
                            Message = $"Conference '{conference.ConferenceName}' đã bị hội đồng từ chối. Vui lòng xem biên bản tại: {documentUrl}",
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
                    var emailSubject = $"[LRMS] Thông báo: Tài liệu mới cho Conference - {conference.ConferenceName}";
                    var emailContent = member.Role == (int)GroupMemberRoleEnum.Stakeholder
                        ? ConferenceEmailTemplates.GetStakeholderConferenceDocumentEmail(member.User, conference.Project, conference, uploader, documentFile.FileName, documentUrl)
                        : ConferenceEmailTemplates.GetMemberConferenceDocumentEmail(member.User, conference.Project, conference, uploader, documentFile.FileName, documentUrl);

                    await _emailService.SendEmailAsync(member.User.Email, emailSubject, emailContent);

                    // Gửi notification cho thành viên thường
                    if (member.Role != (int)GroupMemberRoleEnum.Stakeholder)
                    {
                        string title = member.UserId.Value == userId
                            ? "Bạn đã tải lên tài liệu mới"
                            : "Có tài liệu mới trong Conference";

                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.UserId.Value,
                            Title = title,
                            Message = $"Conference: {conference.ConferenceName}\nTài liệu: {documentFile.FileName}\nNgười tải lên: {uploader?.FullName}",
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
} 