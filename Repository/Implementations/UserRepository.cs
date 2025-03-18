using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;
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
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Email.Equals(email.ToLower()));
    }
}
