using Domain.Constants;
using Domain.DTO.Requests;
using LRMS_API;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using System.Text;

namespace Service.Implementations;
public class ConferenceService : IConferenceService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IS3Service _s3Service;
    private readonly LRMSDbContext _context;

    public ConferenceService(
        IProjectRepository projectRepository,
        IGroupRepository groupRepository,
        INotificationService notificationService,
        IS3Service s3Service,
        LRMSDbContext context,
        IEmailService emailService)
    {
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
        _s3Service = s3Service;
        _context = context;
        _emailService = emailService;
    }

    public async Task<int> CreateConferenceFromResearch(int projectId, int leaderId, CreateConferenceFromProjectRequest request, IFormFile documentFile)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Milestones)
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

            if (project.Milestones.Any(m => m.Status != (int)MilestoneStatusEnum.Completed))
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
                    DocumentType = (int)DocumentTypeEnum.ConferenceSubmission,
                    UploadAt = DateTime.Now,
                    UploadedBy = leaderId,
                    ProjectResourceId = projectResource.ProjectResourceId
                };

                await _context.Documents.AddAsync(document);
            }

            await _context.SaveChangesAsync();

            var groupMembers = await _groupRepository.GetMembersByGroupId(project.GroupId.Value);
            // Tạo thông tin chi tiết về conference
            var conferenceInfo = new StringBuilder();
            conferenceInfo.AppendLine($"Thông tin chi tiết về Conference:");
            conferenceInfo.AppendLine($"- Tên Conference: {conference.ConferenceName}");
            conferenceInfo.AppendLine($"- Địa điểm: {conference.Location}");
            conferenceInfo.AppendLine($"- Xếp hạng: {conference.ConferenceRanking}");
            conferenceInfo.AppendLine($"- Ngày thuyết trình: {conference.PresentationDate:dd/MM/yyyy}");
            conferenceInfo.AppendLine($"- Hình thức: {conference.PresentationType}");

            foreach (var member in groupMembers.Where(m => 
            m.Status == (int)GroupMemberStatus.Active &&
            m.Role != (int)GroupMemberRoleEnum.Stakeholder))
            {
                if (member.UserId.HasValue)
                {
                    await _notificationService.CreateNotification(new CreateNotificationRequest
                    {
                        UserId = member.UserId.Value,
                        Title = "Đăng ký tham gia Conference mới",
                        Message = $"Project đã được chuyển thành Conference và đang chờ phê duyệt: {request.ConferenceName}",
                        ProjectId = projectId,
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
                $"Thông báo đăng ký Conference mới",
                $"Xin chào {stakeholder.User.FullName},\n\n" +
                $"Dự án nghiên cứu đã được đăng ký tham gia Conference.\n\n" +
                $"{conferenceInfo}\n\n" +
                $"Trân trọng."
                );
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
            DocumentType = (int)DocumentTypeEnum.ConferenceSubmission,
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
}
