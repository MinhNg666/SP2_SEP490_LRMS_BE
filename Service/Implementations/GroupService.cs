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
            var activeMembers = groupMembers.Where(m => m.Status == (int)GroupMemberStatus.Active).ToList();
            
            var groupResponse = _mapper.Map<GroupResponse>(group);
            groupResponse.Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(activeMembers);
            groupResponse.CurrentMember = activeMembers.Count;
            
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
            var groups = await _groupRepository.GetGroupsByUserId(userId);
            if (groups == null || !groups.Any())
            {
                return new List<GroupResponse>();
            }

            var groupResponses = new List<GroupResponse>();
            foreach (var group in groups)
            {
                var members = await _groupRepository.GetMembersByGroupId(group.GroupId);
                
                // For normal users, show only active and pending members
                // For the group creator, also show rejected members
                var relevantMembers = group.CreatedBy == userId
                    ? members.Where(m => 
                        m.Status == (int)GroupMemberStatus.Active || 
                        m.Status == (int)GroupMemberStatus.Pending ||
                        m.Status == (int)GroupMemberStatus.Rejected)
                    : members.Where(m => 
                        m.Status == (int)GroupMemberStatus.Active || 
                        m.Status == (int)GroupMemberStatus.Pending);
                
                var groupResponse = _mapper.Map<GroupResponse>(group);
                groupResponse.Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(relevantMembers);
                
                // Count only active members for CurrentMember
                groupResponse.CurrentMember = members.Count(m => m.Status == (int)GroupMemberStatus.Active);
                
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
        // Validate group name
        if (string.IsNullOrEmpty(request.GroupName))
        {
            throw new ServiceException("Group name cannot be null or empty.");
        }

        // Validate member list
        if (request.Members == null || !request.Members.Any())
        {
            throw new ServiceException("Members cannot be null or empty.");
        }

        // Get creator information
        var creator = await _userRepository.GetByIdAsync(currentUserId);
        if (creator == null)
        {
            throw new ServiceException("Creator not found.");
        }

        bool isStudentCreated = creator.Role == (int)SystemRoleEnum.Student;
        
        // Validate all members
        foreach (var member in request.Members)
        {
            await ValidateMemberInfo(member.MemberEmail, member.MemberName);
        }

        // Count supervisors and students
        var supervisors = request.Members.Where(m => m.Role == (int)GroupMemberRoleEnum.Supervisor).ToList();
        var students = request.Members.Where(m => m.Role != (int)GroupMemberRoleEnum.Supervisor).ToList();
        var leaders = request.Members.Where(m => m.Role == (int)GroupMemberRoleEnum.Leader).ToList();
        
        // Apply business rules based on creator type
        int maxMember;
        if (isStudentCreated)
        {
            // Student-created: Max 2 supervisors + 5 students
            if (supervisors.Count > 2)
            {
                throw new ServiceException("Student-created research groups can have at most 2 supervisors.");
            }
            
            if (supervisors.Count == 0)
            {
                throw new ServiceException("Research group must have at least one supervisor.");
            }
            
            if (students.Count < 4 || students.Count > 5)
            {
                throw new ServiceException("Student-created research groups must have between 4 and 5 student members.");
            }
            
            // Set proper max member value
            maxMember = 7; // 2 supervisors + 5 students
        }
        else
        {
            // Lecturer-created: Max 10 members total
            if (request.Members.Count > 10)
            {
                throw new ServiceException("Lecturer-created research groups can have at most 10 members.");
            }
            
            if (supervisors.Any())
            {
                throw new ServiceException("Lecturer-created research groups cannot have supervisors.");
            }
            
            // Set proper max member value
            maxMember = 10;
        }

        // Ensure exactly one leader
        if (leaders.Count != 1)
        {
            throw new ServiceException("Research group must have exactly one leader.");
        }

        // Verify supervisors are lecturers (for student-created groups)
        if (isStudentCreated)
        {
            foreach (var supervisorMember in supervisors)
            {
                var supervisor = await _userRepository.GetUserByEmail(supervisorMember.MemberEmail);
                if (supervisor.Role != (int)SystemRoleEnum.Lecturer)
                {
                    throw new ServiceException($"Supervisor {supervisorMember.MemberName} must be a lecturer.");
                }
            }
        }

        // Create the group
        var group = new LRMS_API.Group
        {
            GroupName = request.GroupName,
            MaxMember = maxMember,
            CurrentMember = 0,
            Status = 1,
            CreatedAt = DateTime.Now,
            CreatedBy = currentUserId,
            GroupType = (int)GroupTypeEnum.Student
        };
        
        await _groupRepository.AddAsync(group);
        
        // Process members and send invitations
        foreach (var member in request.Members)
        {
            var user = await _userRepository.GetUserByEmail(member.MemberEmail);
            if (user != null)
            {
                // Create GroupMember with status based on if it's the creator
                var isCreator = user.UserId == currentUserId;
                var groupMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = isCreator ? (int?)GroupMemberStatus.Active : (int?)GroupMemberStatus.Pending,
                    JoinDate = isCreator ? DateTime.Now : null
                };
                await _groupRepository.AddMemberAsync(groupMember);
                
                // Only send invitations to non-creators
                if (!isCreator)
                {
                    var invitationRequest = new SendInvitationRequest
                    {
                        Content = $"You have been invited to join the research group '{group.GroupName}'.",
                        InvitedUserId = user.UserId,
                        InvitedBy = currentUserId,
                        GroupId = group.GroupId,
                        InvitedRole = member.Role,
                        ProjectId = null
                    };

                    await _invitationService.SendInvitation(invitationRequest);
                }
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
        
        // Validate council members must be lecturer
        var user = await _userRepository.GetUserByEmail(member.MemberEmail);
        if (user.Role != (int)SystemRoleEnum.Lecturer)
        {
            throw new ServiceException($"Council member {member.MemberName} must be a lecturer.");
        }
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
                // Create GroupMember with status based on if it's the creator
                var isCreator = user.UserId == currentUserId;
                var groupMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    Role = member.Role,
                    UserId = user.UserId,
                    Status = isCreator ? (int?)GroupMemberStatus.Active : (int?)GroupMemberStatus.Pending,
                    JoinDate = isCreator ? DateTime.Now : null
                };

                await _groupRepository.AddMemberAsync(groupMember);

                // Only send invitations to non-creators
                if (!isCreator)
                {
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

    private async Task UpdateGroupMemberCount(int groupId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group != null)
        {
            var activeMembers = await _groupRepository.GetMembersByGroupId(groupId);
            group.CurrentMember = activeMembers.Count(m => m.Status == (int)GroupMemberStatus.Active);
            await _groupRepository.UpdateAsync(group);
        }
    }
}
