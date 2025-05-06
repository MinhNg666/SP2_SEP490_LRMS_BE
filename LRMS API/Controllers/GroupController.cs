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
    [HttpGet("groups/get-group-by-groupId/{groupid}")]
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

    [HttpGet("groups/get-group-by-userId/{userId}")]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetGroupsByUserId(int userId)
    {
        try
        {
            var groups = await _groupService.GetGroupsByUserId(userId);
            return Ok(groups);
        }
        catch (ServiceException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }      
    }
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
    [HttpGet("council-groups")]
    [Authorize] // You may want to restrict this to certain roles
    public async Task<IActionResult> GetAllCouncilGroups()
    {
        try
        {
            var result = await _groupService.GetAllCouncilGroups();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("groups/re-invite")]
    [Authorize]
    public async Task<IActionResult> ReInviteMember(ReInviteMemberRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "User ID is missing from claims."));
            }
            
            var currentUserId = int.Parse(userIdClaim);
            await _groupService.ReInviteMember(request, currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation sent successfully."));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("users/{userId}/groups/basic")]
    [Authorize]
    public async Task<IActionResult> GetUserGroupsBasicInfo(int userId)
    {
        try
        {
            // Check if the user is requesting their own groups or if they're an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserId != userId && userRole != SystemRoleEnum.Admin.ToString())
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new ApiResponse(StatusCodes.Status403Forbidden, "You are not authorized to view this user's groups."));
            }
            
            var groups = await _groupService.GetUserGroupsBasicInfo(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "User groups retrieved successfully", groups));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("users/me/groups/basic")]
    [Authorize]
    public async Task<IActionResult> GetMyGroupsBasicInfo()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var groups = await _groupService.GetUserGroupsBasicInfo(currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Your groups retrieved successfully", groups));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
