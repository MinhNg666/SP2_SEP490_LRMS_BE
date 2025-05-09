using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Repository.Implementations;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using Service.Settings;

namespace Service.Implementations;
public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public InvitationService(IInvitationRepository invitationRepository, IMapper mapper, IGroupRepository groupRepository,
        IEmailService emailService, INotificationService notificationService, IUserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task SendInvitation(SendInvitationRequest request)
    {
        // Get the group first
        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        if (group == null)
        {
            throw new ServiceException("Group not found.");
        }

        // Get the sender's information
        var sender = await _userRepository.GetByIdAsync(request.InvitedBy);
        if (sender == null)
        {
            throw new ServiceException("Sender not found.");
        }

        // Get invited user info
        var invitedUser = await _userRepository.GetByIdAsync(request.InvitedUserId);
        if (invitedUser == null)
        {
            throw new ServiceException("Invited user not found.");
        }

        var invitation = new Invitation
        {
            Message = request.Content,
            RecieveBy = request.InvitedUserId,
            GroupId = request.GroupId,
            SentBy = request.InvitedBy,
            CreatedAt = DateTime.Now,
            Status = (int)InvitationEnum.Pending,
            InvitedRole = request.InvitedRole
        };

        await _invitationRepository.AddInvitationAsync(invitation);

        // Gửi email thông báo cho người được mời
        var emailContent = InvitationEmailTemplates.GetInvitationEmail(invitedUser, group, sender);
        await _emailService.SendEmailAsync(
            invitedUser.Email,
            $"[LRMS] Notification: Group Membership Invitation - {group.GroupName}",
            emailContent
        );

        // Tạo notification cho người được mời
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = request.InvitedUserId,
            Title = "Group Membership Invitation", // Đã thay đổi
            Message = $"You have received an invitation to join the research group {group.GroupName} from {sender.FullName}", // Đã thay đổi
            ProjectId = request.ProjectId,
            Status = 0,
            IsRead = false,
            InvitationId = invitation.InvitationId
        };

        await _notificationService.CreateNotification(notificationRequest);
    }

    public async Task<IEnumerable<InvitationResponse>> GetInvitationsByUserId(int userId)
    {
        var invitations = await _invitationRepository.GetInvitationsByUserId(userId);
        return _mapper.Map<IEnumerable<InvitationResponse>>(invitations);
    }
    public async Task AcceptInvitation(int invitationId, int userId)
    {
        var invitation = await _invitationRepository.GetInvitationById(invitationId);
        if (invitation == null || invitation.RecieveBy != userId)
        {
            throw new ServiceException("Invitation not found or does not belong to the user.");
        }
        
        if (invitation.Status != (int)InvitationEnum.Pending)
        {
            throw new ServiceException("Invitation has already been processed.");
        }

        // Get necessary information for email
        var group = await _groupRepository.GetByIdAsync(invitation.GroupId.Value);
        var member = await _userRepository.GetByIdAsync(userId);
        var leader = await _userRepository.GetByIdAsync(invitation.SentBy.Value);

        invitation.Status = (int)InvitationEnum.Accepted;
        invitation.RespondDate = DateTime.Now;
        await _invitationRepository.UpdateInvitation(invitation);

        // Update notification status
        await _notificationService.UpdateNotificationForInvitation(invitationId, (int)InvitationEnum.Accepted);

        if (invitation.GroupId.HasValue)
        {
            var groupMember = await _groupRepository.GetGroupMember(invitation.GroupId.Value, userId);
            if (groupMember != null)
            {
                groupMember.Status = (int)GroupMemberStatus.Active;
                groupMember.JoinDate = DateTime.Now;
                await _groupRepository.UpdateMemberAsync(groupMember);

                // Gửi email thông báo cho leader
                var emailContent = InvitationEmailTemplates.GetAcceptInvitationEmail(member, group, leader);
                await _emailService.SendEmailAsync(
                    leader.Email,
                    $"[LRMS] Notification: New Member Has Joined Group - {group.GroupName}",
                    emailContent
                );

                // Tạo notification cho leader
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = leader.UserId,
                    Title = $"New Member in Group {group.GroupName}", // Đã thay đổi
                    Message = $"{member.FullName} has accepted the invitation to join the group", // Đã thay đổi
                    Status = 0,
                    IsRead = false,
                    InvitationId = invitationId
                };
                await _notificationService.CreateNotification(notificationRequest);
                
                // Check if all members have accepted their invitations
                await CheckAndUpdateGroupStatus(invitation.GroupId.Value);
            }
        }
        else
        {
            throw new ServiceException("Group ID is missing from invitation.");
        }
    }

    // New method to check if all members have accepted and update group status
    private async Task CheckAndUpdateGroupStatus(int groupId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null)
            return;
        
        // Only check groups that are in Pending status
        if (group.Status != (int)GroupStatusEnum.Pending)
            return;
            
        // Get all members for this group
        var members = await _groupRepository.GetMembersByGroupId(groupId);
        
        // Check if all non-creator members have accepted (status is Active) or they've been rejected
        bool allMembersResolved = true;
        foreach (var member in members)
        {
            // Skip the creator who is automatically active
            if (member.UserId == group.CreatedBy)
                continue;
                
            // If there are any pending members, the group is not ready to be activated
            if (member.Status == (int)GroupMemberStatus.Pending)
            {
                allMembersResolved = false;
                break;
            }
        }
        
        // If all invitations have been either accepted or rejected, update the group status
        if (allMembersResolved)
        {
            group.Status = (int)GroupStatusEnum.Active;
            await _groupRepository.UpdateAsync(group);
        }
    }

    public async Task RejectInvitation(int invitationId, int userId)
    {
        var invitation = await _invitationRepository.GetInvitationById(invitationId);
        if (invitation == null || invitation.RecieveBy != userId)
        {
            throw new ServiceException("Invitation not found or does not belong to the user.");
        }

        if (invitation.Status != (int)InvitationEnum.Pending)
        {
            throw new ServiceException("Invitation has already been processed.");
        }

        // Get necessary information for email
        var group = await _groupRepository.GetByIdAsync(invitation.GroupId.Value);
        var member = await _userRepository.GetByIdAsync(userId);
        var leader = await _userRepository.GetByIdAsync(invitation.SentBy.Value);

        invitation.Status = (int)InvitationEnum.Rejected;
        invitation.RespondDate = DateTime.Now;
        await _invitationRepository.UpdateInvitation(invitation);

        // Update notification status
        await _notificationService.UpdateNotificationForInvitation(invitationId, (int)InvitationEnum.Rejected);

        if (invitation.GroupId.HasValue)
        {
            // Update member status to rejected
            var groupMember = await _groupRepository.GetGroupMember(invitation.GroupId.Value, userId);
            if (groupMember != null)
            {
                groupMember.Status = (int)GroupMemberStatus.Rejected;
                await _groupRepository.UpdateMemberAsync(groupMember);

                // Gửi email thông báo cho leader
                var emailContent = InvitationEmailTemplates.GetRejectInvitationEmail(member, group, leader);
                await _emailService.SendEmailAsync(
                    leader.Email,
                    $"[LRMS] Notification: Group Invitation Response - {group.GroupName}",
                    emailContent
                );

                // Tạo notification cho leader
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = leader.UserId,
                    Title = $"Group Invitation Response - {group.GroupName}", // Đã thay đổi
                    Message = $"{member.FullName} has declined the invitation to join the group", // Đã thay đổi
                    Status = 0,
                    IsRead = false,
                    InvitationId = invitationId
                };
                await _notificationService.CreateNotification(notificationRequest);
            }
        }
    }
}
