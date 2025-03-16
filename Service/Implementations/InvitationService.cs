using System;
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
    private readonly IMapper _mapper;
    public InvitationService(IInvitationRepository invitationRepository, IMapper mapper, IGroupRepository groupRepository)
    {
        _invitationRepository = invitationRepository;
        _mapper = mapper;
        _groupRepository = groupRepository;
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
            Status = 0 // 0: Pending
        };

        await _invitationRepository.AddInvitationAsync(invitation);
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

        invitation.Status = 1; // 1: Accepted
        await _invitationRepository.UpdateInvitation(invitation);
        var groupMember = new GroupMember
        {
            GroupId = invitation.GroupId, // Giả sử bạn đã thêm GroupId vào Invitation
            GroupMemberId = invitation.RecieveBy.Value, // Thêm GroupMemberId
            Role = invitation.InvitedRole, // Thêm vai trò
            UserId = userId,
            Status = 1, // Hoạt động
            JoinDate = DateTime.Now
        };
        await _groupRepository.AddMemberAsync(groupMember);
    }

    public async Task RejectInvitation(int invitationId, int userId)
    {
        var invitation = await _invitationRepository.GetInvitationById(invitationId);
        if (invitation == null || invitation.RecieveBy != userId)
        {
            throw new ServiceException("Invitation not found or does not belong to the user.");
        }

        invitation.Status = 2; // 2: Rejected
        await _invitationRepository.UpdateInvitation(invitation);
    }
}
