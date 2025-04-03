using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Responses;
using Domain.DTO.Requests;
using Domain.Constants;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task RegisterStudent(RegisterStudentRequest request);
        Task<IEnumerable<UserResponse>> GetAllUsers();
        Task<UserResponse> GetUserById(int userId);
        Task<IEnumerable<UserResponse>> GetUsersByLevel(LevelEnum level);
        Task<IEnumerable<StudentResponse>> GetAllStudents();
        Task<IEnumerable<LecturerResponse>> GetAllLecturers();
        Task CreateUser(CreateUserRequest request);
        Task<UserResponse> UpdateUser(int userId, UpdateUserRequest request);
        Task<bool> DeleteUser(int userId);
        Task<StudentProfileResponse> GetStudentProfile(int userId);
        Task<StudentProfileResponse> UpdateStudentProfile (int userId, UpdateStudentRequest request);

    }
}
