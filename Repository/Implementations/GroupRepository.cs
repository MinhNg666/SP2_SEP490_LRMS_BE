using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;

public class GroupRepository : GenericRepository<Group>, IGroupRepository
{
    public async Task<IEnumerable<GroupMember>> GetMembersByGroupId(int groupId)
    {
        return await _context.GroupMembers
            .AsSplitQuery()
            .Where(x => x.GroupId == groupId)
            .ToListAsync();
    }

    public async Task AddMemberAsync(GroupMember groupMember)
    {
        await _context.GroupMembers.AddAsync(groupMember); // Thêm dòng này
        await _context.SaveChangesAsync(); // Lưu thay đổi
    }
}

