using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LRMS_API.Controllers;
[ApiController]

public class UserController : ApiBaseController
{
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;
    public UserController(IUserService userService, IGroupService groupService)
    {
        _userService = userService;
        _groupService = groupService;
    }
    [HttpGet("users")]
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
    [HttpGet("users/{userId}")]
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
    [HttpGet("users/level/{level}")]
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
    [HttpPut("users/{userId}")]
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
    [HttpGet("users/students")]
    [Authorize]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            var students = await _userService.GetAllStudents();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, students));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("users/lecturers")]
    [Authorize]
    public async Task<IActionResult> GetAllLecturers()
    {
        try
        {
            var lecturers = await _userService.GetAllLecturers();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, lecturers));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("users")]
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
    [HttpDelete("users/{userId}")]
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
    [HttpGet("users/{userId}/profile")]
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
    [HttpPut("users/{userId}/profile")]
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
    [HttpGet("users/academic")]
    [Authorize]
    public async Task<IActionResult> GetAcademicUsers()
    {
        try
        {
            var students = await _userService.GetAllStudents();
            var lecturers = await _userService.GetAllLecturers();
            
            var academicUsers = new 
            {
                Students = students,
                Lecturers = lecturers
            };
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, academicUsers));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("users/{userId}/groups")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetUserGroups(int userId)
    {
        try
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            
            var currentUserId = int.Parse(currentUserIdString);
            
            if (currentUserId != userId)
            {
                return Forbid();
            }
            
            var groups = await _groupService.GetGroupsByUserId(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, groups));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ApiResponse(StatusCodes.Status500InternalServerError, "An error occurred while processing your request."));
        }      
    }
}
