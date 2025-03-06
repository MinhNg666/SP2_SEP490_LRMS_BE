using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;
public class GroupService : IGroupService
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;


    public GroupService(IUserRepository userRepository, IGroupRepository groupRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _mapper = mapper;
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
    public async Task CreateStudentGroup(CreateStudentGroupRequest request)
    {
        var group = new Group
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
                    MemberName = member.MemberName,
                    MemberEmail = member.MemberEmail,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = 1,
                    JoinDate = DateTime.Now
                };
                await _groupRepository.AddMemberAsync(groupMember);
            }
        }
    }
    public async Task CreateCouncilGroup(CreateCouncilGroupRequest request)
    {
        var group = new Group
        {
            GroupName = request.GroupName,
            MaxMember = request.MaxMember,
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
                    MemberName = member.MemberName,
                    MemberEmail = member.MemberEmail,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = 1,
                    JoinDate = DateTime.Now
                };

                await _groupRepository.AddMemberAsync(groupMember);
            }
        }
    }
}
