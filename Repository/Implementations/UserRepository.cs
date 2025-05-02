using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;
public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly LRMSDbContext _context;

    // Phải gọi base constructor
    public UserRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<User> GetUserByEmail(string email)
    {
        return await _context.Users
            .Include(u => u.Department)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Email.Equals(email.ToLower()));
    }

    public async Task<IEnumerable<UserGroupResponse>> GetUserGroups(int userId)
    {
        return await _context.GroupMembers
            .Include(gm => gm.Group)
            .Include(gm => gm.Group.GroupDepartmentNavigation)
            .Where(gm => gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active)
            .Select(gm => new UserGroupResponse
            {
                GroupId = gm.GroupId,
                GroupName = gm.Group.GroupName,
                Role = gm.Role,
                GroupType = gm.Group.GroupType,
                DepartmentId = gm.Group.GroupType == (int)GroupTypeEnum.Council ? gm.Group.GroupDepartment : null,
                DepartmentName = gm.Group.GroupType == (int)GroupTypeEnum.Council && gm.Group.GroupDepartmentNavigation != null 
                    ? gm.Group.GroupDepartmentNavigation.DepartmentName 
                    : null,
                Status = gm.Group.Status
            }).ToListAsync();
    }

    public async Task<User> GetUserByRefreshToken(string refreshToken)
    {
        return await _context.Users
            .Include(u => u.Department)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.RefreshToken == refreshToken);
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Department)
            .ToListAsync();
    }
}
