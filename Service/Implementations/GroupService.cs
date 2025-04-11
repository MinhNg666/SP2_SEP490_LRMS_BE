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
    private readonly IInvitationRepository _invitationRepository;

    public GroupService(IUserRepository userRepository, IGroupRepository groupRepository, 
        IMapper mapper, IInvitationService invitationService, IInvitationRepository invitationRepository)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _mapper = mapper;
        _invitationService = invitationService;
        _invitationRepository = invitationRepository;
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
                
                // Set the department name if available
                if (group.GroupDepartmentNavigation != null)
                {
                    groupResponse.DepartmentName = group.GroupDepartmentNavigation.DepartmentName;
                }
                
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
            throw new ServiceException("Council must have one chairman.");
        }

        if (secretaryCount != 1)
        {
            throw new ServiceException("Council must have one secretary.");
        }

        if (councilMemberCount != 3)
        {
            throw new ServiceException("Council must have three council members.");
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
                        Content = $"You have been invited to join the council group '{group.GroupName}'.",
                        InvitedUserId = user.UserId,
                        InvitedBy = currentUserId,
                        InvitedRole = member.Role,
                        GroupId = group.GroupId,
                        ProjectId = null 
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

    public async Task<IEnumerable<GroupResponse>> GetAllCouncilGroups()
    {
        try
        {
            var groups = await _groupRepository.GetAllAsync();
            
            // Filter out only council groups (GroupType = 1)
            var councilGroups = groups.Where(g => g.GroupType == (int)GroupTypeEnum.Council).ToList();
            
            if (!councilGroups.Any())
            {
                return new List<GroupResponse>();
            }
            
            var groupResponses = new List<GroupResponse>();
            
            foreach (var group in councilGroups)
            {
                var members = await _groupRepository.GetMembersByGroupId(group.GroupId);
                
                // Include all members regardless of status
                var groupResponse = _mapper.Map<GroupResponse>(group);
                groupResponse.Members = _mapper.Map<IEnumerable<GroupMemberResponse>>(members);
                
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

    public async Task ReInviteMember(ReInviteMemberRequest request, int currentUserId)
    {
        try
        {
            // Check if group exists
            var group = await _groupRepository.GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new ServiceException("Group not found.");
            }

            // Verify authorization
            bool isAuthorized = group.CreatedBy == currentUserId;
            if (!isAuthorized)
            {
                var userMembership = await _groupRepository.GetGroupMember(request.GroupId, currentUserId);
                isAuthorized = userMembership != null && 
                              userMembership.Status == (int)GroupMemberStatus.Active && 
                              (userMembership.Role == (int)GroupMemberRoleEnum.Leader || 
                               userMembership.Role == (int)GroupMemberRoleEnum.Supervisor);
            }
            if (!isAuthorized)
            {
                throw new ServiceException("You do not have permission to invite members to this group.");
            }

            // Validate member info
            await ValidateMemberInfo(request.MemberEmail, request.MemberName);
            var invitedUser = await _userRepository.GetUserByEmail(request.MemberEmail);

            // Get all group members and pending invitations
            var groupMembers = await _groupRepository.GetMembersByGroupId(request.GroupId);
            var pendingInvitations = await _invitationRepository.GetPendingInvitationsByGroupId(request.GroupId);

            if (group.GroupType == (int)GroupTypeEnum.Council)
            {
                // Count current active members for each role
                int activeChairmanCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Council_Chairman);
                
                int activeSecretaryCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Secretary);
                
                int activeCouncilMemberCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Council_Member);

                // Count pending invitations for each role
                int pendingChairmanCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Council_Chairman);
                
                int pendingSecretaryCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Secretary);
                
                int pendingCouncilMemberCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Council_Member);

                // Validate based on role
                switch (request.Role)
                {
                    case (int)GroupMemberRoleEnum.Council_Chairman:
                        if (activeChairmanCount + pendingChairmanCount >= 1)
                        {
                            throw new ServiceException("Cannot invite more chairman. The position is already filled or has a pending invitation.");
                        }
                        break;

                    case (int)GroupMemberRoleEnum.Secretary:
                        if (activeSecretaryCount + pendingSecretaryCount >= 1)
                        {
                            throw new ServiceException("Cannot invite more secretary. The position is already filled or has a pending invitation.");
                        }
                        break;

                    case (int)GroupMemberRoleEnum.Council_Member:
                        if (activeCouncilMemberCount + pendingCouncilMemberCount >= 3)
                        {
                            throw new ServiceException("Cannot invite more council members. All positions are filled or have pending invitations.");
                        }
                        break;

                    default:
                        throw new ServiceException("Invalid role for council group.");
                }

                // Validate that invited user is a lecturer
                if (invitedUser.Role != (int)SystemRoleEnum.Lecturer)
                {
                    throw new ServiceException("Council members must be lecturers.");
                }
            }
            else if (group.GroupType == (int)GroupTypeEnum.Student) 
            {
                // Get information about the creator
                var creator = await _userRepository.GetByIdAsync(group.CreatedBy.Value);
                bool isStudentCreated = creator.Role == (int)SystemRoleEnum.Student;
                
                // Count current active members by role
                int activeSupervisorCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Supervisor);
                
                int activeLeaderCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Leader);
                
                int activeMemberCount = groupMembers.Count(m => 
                    m.Status == (int)GroupMemberStatus.Active && 
                    m.Role == (int)GroupMemberRoleEnum.Member);
                
                // Count pending invitations by role
                int pendingSupervisorCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Supervisor);
                
                int pendingLeaderCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Leader);
                
                int pendingMemberCount = pendingInvitations.Count(i => 
                    i.InvitedRole == (int)GroupMemberRoleEnum.Member);
                
                // Calculate total student members (leaders + regular members)
                int totalActiveStudentMembers = activeLeaderCount + activeMemberCount;
                int totalPendingStudentMembers = pendingLeaderCount + pendingMemberCount;
                
                // Different validations based on group creator
                if (isStudentCreated)
                {
                    // Student-created group: max 2 supervisors + 5 students (1 leader + 4 members)
                    // Validate based on role
                    switch (request.Role)
                    {
                        case (int)GroupMemberRoleEnum.Supervisor:
                            // Check if invited user is a lecturer
                            if (invitedUser.Role != (int)SystemRoleEnum.Lecturer)
                            {
                                throw new ServiceException("Supervisors must be lecturers.");
                            }
                            
                            // Check supervisor count
                            if (activeSupervisorCount + pendingSupervisorCount >= 2)
                            {
                                throw new ServiceException("Cannot invite more supervisors. The group already has the maximum number of supervisors (2) active or pending.");
                            }
                            break;
                            
                        case (int)GroupMemberRoleEnum.Leader:
                            // Check if there's already a leader
                            if (activeLeaderCount + pendingLeaderCount >= 1)
                            {
                                throw new ServiceException("Cannot invite more leaders. The group already has a leader active or pending.");
                            }
                            
                            // Check student count (including this new leader)
                            if (totalActiveStudentMembers + totalPendingStudentMembers >= 5)
                            {
                                throw new ServiceException("Cannot invite more student members. The group already has the maximum number of student members (5) active or pending.");
                            }
                            break;
                            
                        case (int)GroupMemberRoleEnum.Member:
                            // Check student count
                            if (totalActiveStudentMembers + totalPendingStudentMembers >= 5)
                            {
                                throw new ServiceException("Cannot invite more members. The group already has the maximum number of student members (5) active or pending.");
                            }
                            break;
                            
                        default:
                            throw new ServiceException("Invalid role for student research group.");
                    }
                }
                else
                {
                    // Lecturer-created group: max 10 members (1 leader + 9 members)
                    // Total member count (all roles)
                    int totalActiveMembers = groupMembers.Count(m => m.Status == (int)GroupMemberStatus.Active);
                    int totalPendingMembers = pendingInvitations.Count();
                    
                    // Validate total count first
                    if (totalActiveMembers + totalPendingMembers >= 10)
                    {
                        throw new ServiceException("Cannot invite more members. The group already has the maximum number of members (10) active or pending.");
                    }
                    
                    // Validate based on role
                    switch (request.Role)
                    {
                        case (int)GroupMemberRoleEnum.Leader:
                            if (activeLeaderCount + pendingLeaderCount >= 1)
                            {
                                throw new ServiceException("Cannot invite more leaders. The group already has a leader active or pending.");
                            }
                            break;
                            
                        case (int)GroupMemberRoleEnum.Supervisor:
                            throw new ServiceException("Lecturer-created research groups cannot have supervisors.");
                            
                        case (int)GroupMemberRoleEnum.Member:
                            // Already checked the total count above
                            break;
                            
                        default:
                            throw new ServiceException("Invalid role for student research group.");
                    }
                }
            }

            // Check existing membership
            var groupMember = await _groupRepository.GetGroupMember(request.GroupId, invitedUser.UserId);
            if (groupMember != null)
            {
                if (groupMember.Status == (int)GroupMemberStatus.Active)
                {
                    throw new ServiceException("This user is already an active member of the group.");
                }
                if (groupMember.Status == (int)GroupMemberStatus.Pending)
                {
                    throw new ServiceException("This user already has a pending invitation to this group.");
                }

                groupMember.Status = (int)GroupMemberStatus.Pending;
                groupMember.Role = request.Role;
                await _groupRepository.UpdateMemberAsync(groupMember);
            }
            else
            {
                groupMember = new GroupMember
                {
                    GroupId = request.GroupId,
                    UserId = invitedUser.UserId,
                    Role = request.Role,
                    Status = (int)GroupMemberStatus.Pending,
                    JoinDate = null
                };
                await _groupRepository.AddMemberAsync(groupMember);
            }

            // Send invitation
            string content = !string.IsNullOrEmpty(request.Message)
                ? request.Message
                : $"You have been invited to join the group '{group.GroupName}'.";

            var invitationRequest = new SendInvitationRequest
            {
                Content = content,
                InvitedUserId = invitedUser.UserId,
                InvitedBy = currentUserId,
                GroupId = request.GroupId,
                InvitedRole = request.Role,
                ProjectId = null
            };

            await _invitationService.SendInvitation(invitationRequest);
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
}
