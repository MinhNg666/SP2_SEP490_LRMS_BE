using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Implementations;

namespace Repository.Interfaces;

public interface IGroupRepository : IGenericRepository<Group>
{
    Task<GroupMember> GetMemberByUserId(int userId);
    Task<IEnumerable<GroupMember>> GetMembersByGroupId(int groupId);
    Task AddMemberAsync(GroupMember groupMember);
    Task<Group> GetByIdAsync(int id);
    Task<User> GetUserById(int userId);
    Task<IEnumerable<Group>> GetGroupsByUserId(int userId);
    Task<GroupMember> GetGroupMember(int groupId, int userId);
    Task UpdateMemberAsync(GroupMember groupMember);
    Task DeleteMemberAsync(int groupId, int userId);
}
