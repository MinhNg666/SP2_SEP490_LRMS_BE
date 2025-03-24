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
    Task<IEnumerable<GroupMember>> GetMembersByGroupId(int groupId);
    Task AddMemberAsync(GroupMember groupMember);
    Task<Group> GetByIdAsync(int id);
}
