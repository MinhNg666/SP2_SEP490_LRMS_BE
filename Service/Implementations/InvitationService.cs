﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public InvitationService(IInvitationRepository invitationRepository, IMapper mapper, IGroupRepository groupRepository, INotificationService notificationService)
    {
        _invitationRepository = invitationRepository;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _notificationService = notificationService;
    }

    public async Task SendInvitation(SendInvitationRequest request)
    {
        var invitation = new Invitation
        {
            Message = request.Content,
            RecieveBy = request.InvitedUserId,
            GroupId = request.GroupId, // Thêm GroupId vào đây
            SentBy = request.InvitedBy,
            CreatedAt = DateTime.Now,
            Status = 0, // 0: Pending
            InvitedRole = request.InvitedRole
        };

        await _invitationRepository.AddInvitationAsync(invitation);

        // Tạo notification cho người được mời
        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        var notificationRequest = new CreateNotificationRequest
        {
            UserId = request.InvitedUserId,
            Title = "Group Invitation",
            Message = $"You have been invited to join the group '{group.GroupName}'",
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
        if (invitation.Status != 0) // 2: Rejected
        {
        throw new ServiceException("Invitation has already been proccess.");
        }

        invitation.Status = 1; // 1: Accepted
        invitation.RespondDate = DateTime.Now; // Gán thời gian phản hồi
        await _invitationRepository.UpdateInvitation(invitation);
        var groupMember = await _groupRepository.GetGroupMember(invitation.GroupId.Value, userId);
        if (groupMember != null)
        {
            groupMember.Status = 1; // Active
            groupMember.JoinDate = DateTime.Now;
            await _groupRepository.UpdateMemberAsync(groupMember);
        }
    }

    public async Task RejectInvitation(int invitationId, int userId)
    {
        var invitation = await _invitationRepository.GetInvitationById(invitationId);
        if (invitation == null || invitation.RecieveBy != userId)
        {
            throw new ServiceException("Invitation not found or does not belong to the user.");
        }

        if (invitation.Status != 0) // 2: Rejected
        {
        throw new ServiceException("Invitation has already been proccess.");
        }

        invitation.Status = 2; // 2: Rejected
        await _invitationRepository.UpdateInvitation(invitation);
        await _groupRepository.DeleteMemberAsync(invitation.GroupId.Value, userId);
    }
}
