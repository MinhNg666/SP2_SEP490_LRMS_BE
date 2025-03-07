using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Domain.Constants;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;
[ApiController]

public class UserController : ApiBaseController
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpGet("accounts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers() 
    {
        try 
        {
            var result = await _userService.GetAllUsers();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));

        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("accounts/{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            var result = await _userService.GetUserById(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("accounts/level/{level}")]
    public async Task<IActionResult> GetUsersByLevel(LevelEnum level)
    {
        try
        {
            var result = await _userService.GetUsersByLevel(level);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPut("accounts/{userId}")]
    public async Task<IActionResult> UpdateUser(int userId, UpdateUserRequest request)
    {
        try
        {
            var result = await _userService.UpdateUser(userId, request);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        try
        {
            await _userService.CreateUser(request);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpDelete("accounts/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        try
        {
            var result = await _userService.DeleteUser(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("accounts/{userId}/profile")]
    public async Task<IActionResult> GetStudentProfile(int userId)
    {
        try
        {
            var result = await _userService.GetStudentProfile(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPut("accounts/{userId}/profile")]
    public async Task<IActionResult> UpdateStudentProfile(int userId, UpdateStudentRequest request)
    {
        try
        {
            var result = await _userService.UpdateStudentProfile(userId, request);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
