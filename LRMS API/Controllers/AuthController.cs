using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

namespace LRMS_API.Controllers;

public class AuthController : ApiBaseController
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _userService.Login(request);
            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized,
                    MessageConstants.INVALID_ACCOUNT_CREDENTIALS));
            }

            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPost("auth/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _userService.RefreshToken(request.AccessToken, request.RefreshToken);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
