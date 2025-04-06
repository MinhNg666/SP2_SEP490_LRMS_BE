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
}
