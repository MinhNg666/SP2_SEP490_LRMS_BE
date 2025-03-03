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
                if (BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password))
                {
                    claims.Add(new Claim("UserID", existingUser.UserId.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role,
                        existingUser.RoleId == (int)RoleEnum.Lecturer
                            ? RoleEnum.Lecturer.ToString()
                            : RoleEnum.Admin.ToString()));

                    var loginResponse = _mapper.Map<LoginResponse>(existingUser);
                    loginResponse.AccessToken = _tokenService.GenerateAccessToken(claims);

                    return loginResponse;
                }
            }
            catch (Exception e)
            {
                throw new ServiceException(MessageConstants.NOT_FOUND);
            }
            return null;
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
    }
}
