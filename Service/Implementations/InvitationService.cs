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

namespace Service.Implementations;
public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public InvitationService(IInvitationRepository invitationRepository, IMapper mapper, IGroupRepository groupRepository, INotificationService notificationService, IUserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
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

        // Create notification with sender's name
        var senderName = sender.FullName ?? "Unknown User";
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = request.InvitedUserId,
            Title = "Group Invitation",
            Message = $"You have been invited by {senderName} to join the group '{group.GroupName}'",
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
                groupMember.Status = 1; // Active
                groupMember.JoinDate = DateTime.Now;
                await _groupRepository.UpdateMemberAsync(groupMember);
            }
        }
        else
        {
            throw new ServiceException("Group ID is missing from invitation.");
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
            }
        }
    }
}
