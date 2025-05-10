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
                claims.Add(new Claim(ClaimTypes.NameIdentifier, _adminAccount.Email)); // Thay đổi để sử dụng email admin
                claims.Add(new Claim(ClaimTypes.Role, SystemRoleEnum.Admin.ToString()));
                return new LoginResponse
                {
                    Email = _adminAccount.Email,
                    Status = (int)AccountStatusEnum.Active,
                    Role = (int)SystemRoleEnum.Admin,
                    AccessToken = _tokenService.GenerateAccessToken(claims),
                    Groups = new List<UserGroupResponse>()
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
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString())); // Đảm bảo thêm UserId vào claims
                    claims.Add(new Claim(ClaimTypes.Role,
                        existingUser.Role == (int)SystemRoleEnum.Student
                            ? SystemRoleEnum.Student.ToString()
                            : SystemRoleEnum.Lecturer.ToString()));

                    var loginResponse = _mapper.Map<LoginResponse>(existingUser);
                    
                    // Set Department explicitly if needed
                    if (existingUser.Department != null)
                    {
                        loginResponse.Department = _mapper.Map<DepartmentResponse>(existingUser.Department);
                    }
                    
                    // Generate tokens
                    loginResponse.AccessToken = _tokenService.GenerateAccessToken(claims);
                    loginResponse.RefreshToken = _tokenService.GenerateRefreshToken();
                    loginResponse.TokenExpiresAt = _tokenService.GetAccessTokenExpiryTime();
                    
                    // Update user with refresh token
                    existingUser.RefreshToken = loginResponse.RefreshToken;
                    existingUser.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // 7 days validity
                    existingUser.LastLogin = DateTime.Now;
                    loginResponse.LastLogin = existingUser.LastLogin;
                    
                    await _userRepository.UpdateAsync(existingUser);
                    
                    // Fetch user groups
                    var userGroups = await _userRepository.GetUserGroups(existingUser.UserId);
                    // Fetch if group available
                    if (userGroups.Any())
                    {
                        loginResponse.Groups = userGroups.ToList();
                    }

                    if (existingUser.Role == (int)SystemRoleEnum.Lecturer)
                    {
                        loginResponse.Level = existingUser.Level;
                    }
                    else
                    {
                        loginResponse.Level = null;
                    }

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
                user.Role = (int)SystemRoleEnum.Student;
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
                var userGroups = await _userRepository.GetUserGroups(accountId);
                var userResponse = _mapper.Map<UserResponse>(result);
                userResponse.Groups = userGroups.ToList(); // Sử dụng danh sách UserGroupResponse

                return userResponse;
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
        public async Task<IEnumerable<StudentResponse>> GetAllStudents()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var students = users.Where(u => u.Role == (int)SystemRoleEnum.Student);
                var studentResponses = new List<StudentResponse>();
                
                foreach (var student in students)
                {
                    var studentResponse = _mapper.Map<StudentResponse>(student);
                    studentResponses.Add(studentResponse);
                }
            
                return studentResponses;
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
        public async Task<IEnumerable<LecturerResponse>> GetAllLecturers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var lecturers = users.Where(u => u.Role == (int)SystemRoleEnum.Lecturer);
                
                // Use a list to populate with mapped and enhanced data
                var lecturerResponses = new List<LecturerResponse>();
                
                foreach (var lecturer in lecturers)
                {
                    var lecturerResponse = _mapper.Map<LecturerResponse>(lecturer);
                    
                    // Add department name if department exists
                    if (lecturer.Department != null)
                    {
                        lecturerResponse.DepartmentName = lecturer.Department.DepartmentName;
                    }
                    
                    lecturerResponses.Add(lecturerResponse);
                }
                
                return lecturerResponses;
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
        public async Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId)
        {
            var notifications = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
        }
        public async Task<LoginResponse> RefreshToken(string accessToken, string refreshToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                throw new ServiceException("Invalid client request");
            }
            
            var claims = principal.Claims.ToList();
            
            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _userRepository.UpdateAsync(user);
            
            var loginResponse = _mapper.Map<LoginResponse>(user);
            loginResponse.AccessToken = newAccessToken;
            loginResponse.RefreshToken = newRefreshToken;
            loginResponse.TokenExpiresAt = _tokenService.GetAccessTokenExpiryTime();
            loginResponse.LastLogin = user.LastLogin;
            
            // Fetch user groups
            var userGroups = await _userRepository.GetUserGroups(user.UserId);
            if (userGroups.Any()) // Only set groups if there are any
            {
                loginResponse.Groups = userGroups.ToList();
            }
            
            return loginResponse;
        }
        public async Task<bool> Logout(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ServiceException("Refresh token is required");
            }
            
            var user = await _userRepository.GetUserByRefreshToken(refreshToken);
            if (user == null)
            {
                // hidden token
                return true;
            }
            
            // Clear refresh token data
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            
            await _userRepository.UpdateAsync(user);
            return true;
        }
        public async Task<IEnumerable<ResearcherResponse>> GetAllResearchers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var researchers = users.Where(u => u.Role == (int)SystemRoleEnum.Researcher);
                
                // Create simplified researcher responses without sensitive data
                var researcherResponses = researchers.Select(researcher => new ResearcherResponse 
                {
                    UserId = researcher.UserId,
                    FullName = researcher.FullName,
                    Email = researcher.Email,
                    Phone = researcher.Phone,
                    DepartmentId = researcher.DepartmentId,
                    DepartmentName = researcher.Department?.DepartmentName,
                    Level = researcher.Level?.ToString()
                }).ToList();
                
                return researcherResponses;
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message);
            }
        }
    }
}
