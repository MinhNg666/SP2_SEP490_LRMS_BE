using Domain.DTO.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace LRMS_API.Controllers;
[ApiController]

public class InvitationController : ApiBaseController
{
    private readonly IInvitationService _invitationService;

    public InvitationController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetInvitations(int userId)
    {
        var invitations = await _invitationService.GetInvitationsByUserId(userId);
        return Ok(invitations);
    }
    [HttpPost("accept/{invitationId}")]
    public async Task<IActionResult> AcceptInvitation(int invitationId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Lấy ID người dùng từ Claims
        await _invitationService.AcceptInvitation(invitationId, userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation accepted successfully."));
    }

    [HttpPost("reject/{invitationId}")]
    public async Task<IActionResult> RejectInvitation(int invitationId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Lấy ID người dùng từ Claims
        await _invitationService.RejectInvitation(invitationId, userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Invitation rejected successfully."));
    }
}
