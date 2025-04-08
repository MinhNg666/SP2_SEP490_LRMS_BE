using Domain.DTO.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Domain.DTO.Requests;

namespace LRMS_API.Controllers;
[ApiController]
[Route("api/invitations")]
public class InvitationController : ApiBaseController
{
    private readonly IInvitationService _invitationService;

    public InvitationController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInvitations([FromQuery] int userId)
    {
        var invitations = await _invitationService.GetInvitationsByUserId(userId);
        return Ok(invitations);
    }

    [HttpPatch("{invitationId}")]
    public async Task<IActionResult> UpdateInvitation(int invitationId, [FromBody] UpdateInvitationRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request body is required"));
            }
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Check if Status is string
            if (request.Status is string statusStr)
            {
                if (statusStr.Equals("accepted", StringComparison.OrdinalIgnoreCase))
                {
                    await _invitationService.AcceptInvitation(invitationId, userId);
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation accepted successfully."));
                }
                else if (statusStr.Equals("rejected", StringComparison.OrdinalIgnoreCase))
                {
                    await _invitationService.RejectInvitation(invitationId, userId);
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation rejected successfully."));
                }
            }
            // Check if Status is int
            else if (request.Status is int statusInt)
            {
                if (statusInt == 1)
                {
                    await _invitationService.AcceptInvitation(invitationId, userId);
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation accepted successfully."));
                }
                else if (statusInt == 2)
                {
                    await _invitationService.RejectInvitation(invitationId, userId);
                    return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation rejected successfully."));
                }
            }
            
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid status update"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("{invitationId}/accept")]
    public async Task<IActionResult> AcceptInvitation(int invitationId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated or missing ID claim"));
            }
            
            var userId = int.Parse(userIdClaim);
            await _invitationService.AcceptInvitation(invitationId, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation accepted successfully."));
        }
        catch (FormatException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid user ID format"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("{invitationId}/reject")]
    public async Task<IActionResult> RejectInvitation(int invitationId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated or missing ID claim"));
            }
            
            var userId = int.Parse(userIdClaim);
            await _invitationService.RejectInvitation(invitationId, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation rejected successfully."));
        }
        catch (FormatException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid user ID format"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
