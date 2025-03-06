using AutoMapper;
using Domain.DTO.Responses;
using Domain.DTO.Requests;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using System.Security.Claims;
using Service.Settings;
using Microsoft.Extensions.Options;
using Domain.Constants;
using LRMS_API;
using CloudinaryDotNet;

namespace Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly AdminAccount _adminAccount;


        public UserService(IMapper mapper, IOptions<AdminAccount> adminAccount, IUserRepository userRepository,
            ITokenService tokenService)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _adminAccount = adminAccount.Value;
            _tokenService = tokenService;
        }
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, request.Email)
            };
                if (request.Email.ToLower().Equals(_adminAccount.Email.ToLower()) &&
                request.Password.Equals(_adminAccount.Password))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleEnum.Admin.ToString()));
                    return new LoginResponse
                {
                    Email = _adminAccount.Email,
                    Status = (int)AccountStatusEnum.Active,
                    Role = (int)RoleEnum.Admin,
                    AccessToken = _tokenService.GenerateAccessToken(claims)
                };
            }
            try
            {
                var existingUser = await _userRepository.GetUserByEmail(request.Email);
                if (existingUser == null)
                {
                    throw new ServiceException(MessageConstants.NOT_FOUND);
                }
                if (BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password))
                {
                    claims.Add(new Claim("UserID", existingUser.UserId.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role,
                        existingUser.Role == (int)RoleEnum.Student
                            ? RoleEnum.Student.ToString()
                            : RoleEnum.Lecturer.ToString()));

                    var loginResponse = _mapper.Map<LoginResponse>(existingUser);
                    loginResponse.AccessToken = _tokenService.GenerateAccessToken(claims);

                    return loginResponse;
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                throw new ServiceException(MessageConstants.INVALID_ACCOUNT_CREDENTIALS);
            }
            return null;
        }
        public async Task RegisterStudent(RegisterStudentRequest request) 
        {
            try
            {
                var existingUser = await _userRepository.GetUserByEmail(request.Email);
                if (existingUser != null)
                    throw new ServiceException(MessageConstants.DUPLICATE);

                var user = _mapper.Map<User>(request);
                user.Role = (int)RoleEnum.Student;
                user.Status = (int)AccountStatusEnum.Active;
                user.CreatedAt = DateTime.Now;
                await _userRepository.AddAsync(user);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<IEnumerable<UserResponse>> GetAllUsers()
        {
            try
            {
                var result = await _userRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<UserResponse>>(result);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<UserResponse> GetUserById(int accountId)
        {
            try
            {
                var result = await _userRepository.GetByIdAsync(accountId);
                if (result == null)
                    throw new ServiceException(MessageConstants.NOT_FOUND);

                return _mapper.Map<UserResponse>(result);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<IEnumerable<UserResponse>> GetUsersByLevel(LevelEnum level)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var filteredUsers = users.Where(u => u.Level == (int)level);
                return _mapper.Map<IEnumerable<UserResponse>>(filteredUsers);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task CreateUser(CreateUserRequest request)
        {

            try
            {
                var user = _mapper.Map<User>(request);
                await _userRepository.AddAsync(user);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<UserResponse> UpdateUser(int userId, UpdateUserRequest request)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null)
                    throw new ServiceException(MessageConstants.NOT_FOUND);

                _mapper.Map(request, existingUser);
                await _userRepository.UpdateAsync(existingUser);
                return _mapper.Map<UserResponse>(existingUser);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null)
                    throw new ServiceException(MessageConstants.NOT_FOUND);

                await _userRepository.DeleteAsync(existingUser);
                return true;
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<StudentProfileResponse> GetStudentProfile(int userId)
        {
            try
            {
                var result = await _userRepository.GetByIdAsync(userId);
                if (result == null)
                    throw new ServiceException(MessageConstants.NOT_FOUND);
                return _mapper.Map<StudentProfileResponse>(result);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<StudentProfileResponse> UpdateStudentProfile(int userId, UpdateStudentRequest request)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null)
                    throw new ServiceException(MessageConstants.NOT_FOUND);
                _mapper.Map(request, existingUser);
                await _userRepository.UpdateAsync(existingUser);
                return _mapper.Map<StudentProfileResponse>(existingUser);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
    }
}
