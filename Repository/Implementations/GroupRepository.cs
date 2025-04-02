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
    private readonly LRMSDbContext _context;
    public GroupRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<Group> GetByIdAsync(int id)
    {
        try 
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .SingleOrDefaultAsync(g => g.GroupId == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting group by id: {ex.Message}");
        }
    }

    public override async Task<IEnumerable<Group>> GetAllAsync()
    {
        try
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting all groups: {ex.Message}");
        }
    }

    public async Task<IEnumerable<GroupMember>> GetMembersByGroupId(int groupId)
    {
        try
        {
            return await _context.GroupMembers
                .Include(x => x.User)
                .Where(x => x.GroupId == groupId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting members by group id: {ex.Message}");
        }
    }

    public async Task AddMemberAsync(GroupMember groupMember)
    {
        try
        {
            await _context.GroupMembers.AddAsync(groupMember);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding group member: {ex.Message}");
        }
    }
    public async Task<GroupMember> GetGroupMember(int groupId, int userId)
    {
        return await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
    }

    public async Task UpdateMemberAsync(GroupMember groupMember)
    {
        _context.GroupMembers.Update(groupMember);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMemberAsync(int groupId, int userId)
    {
        var member = await GetGroupMember(groupId, userId);
        if (member != null)
        {
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }
}

