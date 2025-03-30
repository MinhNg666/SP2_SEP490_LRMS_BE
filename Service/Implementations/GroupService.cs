using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
public class GroupService : IGroupService
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IInvitationService _invitationService;

    public GroupService(IUserRepository userRepository, IGroupRepository groupRepository, 
        IMapper mapper, IInvitationService invitationService)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _mapper = mapper;
        _invitationService = invitationService;
    }
    public async Task<IEnumerable<GroupResponse>> GetAllGroups() 
    {
        try
        {
            var group = await _groupRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GroupResponse>>(group);
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
    public async Task<GroupResponse> GetGroupById(int groupId)
    {
        try
         {
             var group = await _groupRepository.GetByIdAsync(groupId);
             if (group == null)
             {
                 throw new ServiceException("Group not found.");
             }
             var groupMembers = await _groupRepository.GetMembersByGroupId(groupId);
             var groupResponse = _mapper.Map<GroupResponse>(group);
             groupResponse.Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(groupMembers);
 
             return groupResponse;
         }
         catch (Exception e)
         {
             throw new ServiceException(e.Message);
         }
    }
    public async Task CreateStudentGroup(CreateStudentGroupRequest request, int currentUserId)
    {
        var group = new LRMS_API.Group
        {
            GroupName = request.GroupName,
            MaxMember = request.MaxMember,
            CurrentMember = 0,
            Status = 1,
            CreatedAt = DateTime.Now,
            GroupType = 0
        };
        await _groupRepository.AddAsync(group);
        // Gửi lời mời cho các thành viên
        foreach (var member in request.Members)
        {
            var user = await _userRepository.GetUserByEmail(member.MemberEmail);
            if (user != null)
            {
                // Tạo GroupMember
                var groupMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = 1,
                };
                await _groupRepository.AddMemberAsync(groupMember);
                var invitationRequest = new SendInvitationRequest
                {
                    Content = $"You have been invited to join the student group '{group.GroupName}'.",
                    InvitedUserId = user.UserId,
                    InvitedBy = currentUserId,
                    GroupId = group.GroupId, // Thêm GroupId vào đây
                    InvitedRole = member.Role,
                    ProjectId = request.ProjectId // Thêm vai trò
                };

                await _invitationService.SendInvitation(invitationRequest);
            }
        }
    }
    public async Task CreateCouncilGroup(CreateCouncilGroupRequest request, int currentUserId)
    {
            if (string.IsNullOrEmpty(request.GroupName))
    {
        throw new ServiceException("Group name cannot be null or empty.");
    }

    if (request.Members == null || !request.Members.Any())
    {
        throw new ServiceException("Members cannot be null or empty.");
    }
        var group = new LRMS_API.Group
        {
            GroupName = request.GroupName,
            GroupDepartment = request.GroupDepartment,
            CreatedBy = currentUserId,
            MaxMember = 5,
            CurrentMember = 0,
            Status = 1,
            CreatedAt = DateTime.Now,
            GroupType = 1
        };

        await _groupRepository.AddAsync(group);

        // Gửi lời mời cho các thành viên
        foreach (var member in request.Members)
        {
            var user = await _userRepository.GetUserByEmail(member.MemberEmail);
            if (user != null)
            {
                // Tạo GroupMember
                var groupMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = (int?)GroupMemberStatus.Pending,
                };

                await _groupRepository.AddMemberAsync(groupMember);

                // Gửi lời mời đến Invitation
                var invitationRequest = new SendInvitationRequest
                {
                    Content = $"You have been invited to join the group '{group.GroupName}'.",
                    InvitedUserId = user.UserId,
                    InvitedBy = currentUserId,
                    InvitedRole = member.Role,
                    GroupId = group.GroupId,
                    ProjectId = request.ProjectId
                };

                await _invitationService.SendInvitation(invitationRequest);
            }
        }
    }
}
