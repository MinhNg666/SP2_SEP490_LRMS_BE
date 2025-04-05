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
    private async Task ValidateMemberInfo(string email, string fullName)
    {
    // Kiểm tra email hợp lệ
        if (string.IsNullOrEmpty(email))
        {
            throw new ServiceException("Email cannot be empty.");
        }

        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            throw new ServiceException($"Invalid email format: {email}");
        }

    // Kiểm tra tên hợp lệ
        if (string.IsNullOrEmpty(fullName))
        {
            throw new ServiceException("Full name cannot be empty.");
        }

    // Kiểm tra user tồn tại trong hệ thống
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null)
        {
            throw new ServiceException($"User with email {email} not found in the system.");
        }

    // Kiểm tra tên khớp với email
        if (!user.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ServiceException($"Full name '{fullName}' does not match with the registered name for email {email}.");
        }
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
    public async Task<IEnumerable<GroupResponse>> GetGroupsByUserId(int userId)
{
    try
    {
        // Lấy danh sách nhóm của user
        var groups = await _groupRepository.GetGroupsByUserId(userId);
        if (groups == null || !groups.Any())
        {
            return new List<GroupResponse>();
        }

        var groupResponses = new List<GroupResponse>();
        foreach (var group in groups)
        {
            // Lấy thông tin thành viên cho mỗi nhóm
            var members = await _groupRepository.GetMembersByGroupId(group.GroupId);
            var groupResponse = _mapper.Map<GroupResponse>(group);
            groupResponse.Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(members);
            groupResponses.Add(groupResponse);
        }

        return groupResponses;
    }
    catch (Exception e)
    {
        throw new ServiceException(e.Message);
    }
}
    public async Task CreateStudentGroup(CreateStudentGroupRequest request, int currentUserId)
    {
        // Kiểm tra tên nhóm
    if (string.IsNullOrEmpty(request.GroupName))
    {
        throw new ServiceException("Group name cannot be null or empty.");
    }

    // Kiểm tra danh sách thành viên
    if (request.Members == null || !request.Members.Any())
    {
        throw new ServiceException("Members cannot be null or empty.");
    }

    // Kiểm tra thông tin của tất cả thành viên
    foreach (var member in request.Members)
    {
        await ValidateMemberInfo(member.MemberEmail, member.MemberName);
    }

    // Kiểm tra số lượng và vai trò của các thành viên
    var leaderCount = request.Members.Count(m => m.Role == (int)GroupMemberRoleEnum.Leader);
    var supervisorCount = request.Members.Count(m => m.Role == (int)GroupMemberRoleEnum.Supervisor);

    if (leaderCount != 1)
    {
        throw new ServiceException("Student group must have exactly one leader.");
    }

    if (supervisorCount != 1)
    {
        throw new ServiceException("Student group must have exactly one supervisor.");
    }

    // Kiểm tra supervisor phải là lecturer
    var supervisorMember = request.Members.First(m => m.Role == (int)GroupMemberRoleEnum.Supervisor);
    var supervisor = await _userRepository.GetUserByEmail(supervisorMember.MemberEmail);
    if (supervisor.Role != (int)SystemRoleEnum.Lecturer)
    {
        throw new ServiceException("Supervisor must be a lecturer.");
    }
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
                    Status = (int?)GroupMemberStatus.Pending,
                    JoinDate = null
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
    foreach (var member in request.Members)
    {
        await ValidateMemberInfo(member.MemberEmail, member.MemberName);
    }

    // Kiểm tra số lượng và vai trò của các thành viên
    var chairmanCount = request.Members.Count(m => m.Role == (int)GroupMemberRoleEnum.Council_Chairman);
    var secretaryCount = request.Members.Count(m => m.Role == (int)GroupMemberRoleEnum.Secretary);
    var councilMemberCount = request.Members.Count(m => m.Role == (int)GroupMemberRoleEnum.Council_Member);

    if (chairmanCount != 1)
    {
        throw new ServiceException("Council must have exactly one chairman.");
    }

    if (secretaryCount != 1)
    {
        throw new ServiceException("Council must have exactly one secretary.");
    }

    if (councilMemberCount != 3)
    {
        throw new ServiceException("Council must have exactly three council members.");
    }

    // Kiểm tra level của từng thành viên
    foreach (var member in request.Members)
    {
        var user = await _userRepository.GetUserByEmail(member.MemberEmail);

        switch (member.Role)
        {
            case (int)GroupMemberRoleEnum.Council_Chairman:
                if (user.Level != (int)LevelEnum.Professor)
                    throw new ServiceException("Council chairman must be a Professor.");
                break;

            case (int)GroupMemberRoleEnum.Secretary:
                if (user.Level != (int)LevelEnum.Associate_Professor)
                    throw new ServiceException("Secretary must be an Associate Professor.");
                break;

            case (int)GroupMemberRoleEnum.Council_Member:
                if (user.Level != (int)LevelEnum.PhD && 
                    user.Level != (int)LevelEnum.Master && 
                    user.Level != (int)LevelEnum.Bachelor)
                    throw new ServiceException("Council member must be PhD, Master or Bachelor.");
                break;
        }
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
                    JoinDate = null
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
