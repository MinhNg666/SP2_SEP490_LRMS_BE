using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Responses;
using LRMS_API;

namespace Repository.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByEmail(string email);
        Task<IEnumerable<UserGroupResponse>> GetUserGroups(int userId); 
        Task<User> GetUserByRefreshToken(string refreshToken);
    }
}
