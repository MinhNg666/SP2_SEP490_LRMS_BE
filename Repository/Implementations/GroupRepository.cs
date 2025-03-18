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
}

