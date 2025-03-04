using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Responses;
using Domain.DTO.Requests;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task<IEnumerable<UserResponse>> GetAllUsers();
        Task<UserResponse> GetUserById(int userId);
        Task CreateUser(CreateUserRequest request);
        Task<UserResponse> UpdateUser(int userId, UpdateUserRequest request);
        Task<bool> DeleteUser(int userId);
        Task<StudentProfileResponse> GetStudentProfile(int userId);
        Task<StudentProfileResponse> UpdateStudentProfile (int userId, UpdateStudentRequest request);

    }
}
