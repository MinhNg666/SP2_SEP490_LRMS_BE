using System.Security.Claims;
using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Implementations;
using Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace LRMS_API.Controllers;
[ApiController]

public class GroupController : ApiBaseController
{
    private readonly IGroupService _groupService;
    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }
    [HttpGet("groups")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllGroups() 
    {
        try
        {
            var result = await _groupService.GetAllGroups();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));

        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("groups/{groupid}")]
    public async Task<IActionResult> GetGroupById(int groupid)
    {
        try
        {
            var result = await _groupService.GetGroupById(groupid);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    // [HttpGet("groups-by-user/{userId}")]
    // [Authorize]
    // public async Task<ActionResult<IEnumerable<GroupResponse>>> GetGroupsByUserId(int userId)
    // {
    //     try
    //     {
    //         var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //         if (string.IsNullOrEmpty(currentUserIdString))
    //         {
    //             return Unauthorized(new { message = "User not authenticated" });
    //         }
            
    //         var currentUserId = int.Parse(currentUserIdString);
            
    //         if (currentUserId != userId)
    //         {
    //             return Forbid();
    //         }
            
    //         var groups = await _groupService.GetGroupsByUserId(userId);
    //         return Ok(groups);
    //     }
    //     catch (ServiceException e)
    //     {
    //         return BadRequest(new { message = e.Message });
    //     }
    //     catch (Exception e)
    //     {
    //         return StatusCode(500, new { message = "An error occurred while processing your request." });
    //     }      
    // }
    [HttpPost("council-groups")]
    public async Task<IActionResult> CreateCouncilGroup(CreateCouncilGroupRequest request) 
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "User ID is missing from claims."));
        }
            var currentUserId = int.Parse(userIdClaim); // Lấy ID người dùng từ Claims
            await _groupService.CreateCouncilGroup(request, currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("research-groups")]
    public async Task<IActionResult> CreateStudentGroup(CreateStudentGroupRequest request) 
    {
        try 
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _groupService.CreateStudentGroup(request, currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
