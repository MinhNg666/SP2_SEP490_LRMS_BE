using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConferenceController : ApiBaseController
{
private readonly IConferenceService _conferenceService;

public ConferenceController(IConferenceService conferenceService)
{
    _conferenceService = conferenceService;
}

[HttpPost("conference/register")]
[Authorize]
public async Task<IActionResult> CreateConference([FromBody] CreateConferenceRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conference = await _conferenceService.CreateConference(request, userId);
        var response = new ApiResponse(StatusCodes.Status200OK, $"Conference has been registered. Conference ID: {conference.ConferenceId}", conference);
        return Ok(response);
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}
[HttpPost("create-conference-from-research/{projectId}")]
[Authorize]
public async Task<IActionResult> CreateFromResearch(int projectId, [FromForm] CreateConferenceFromProjectRequest request, IFormFile documentFile)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conferenceId = await _conferenceService.CreateConferenceFromResearch(projectId, userId, request, documentFile);
        return Ok(new { success = true, conferenceId = conferenceId, message = "Đã tạo Conference thành công" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}
    
[HttpPost("add-document/{conferenceId}")]
[Authorize]
public async Task<IActionResult> AddConferenceDocument(int conferenceId, IFormFile documentFile)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.AddConferenceDocument(conferenceId, userId, documentFile);
        return Ok(new { success = true, message = "Đã thêm tài liệu thành công" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

    [HttpPost("approve-conference/{conferenceId}")]
    [Authorize]
    public async Task<IActionResult> ApproveConference(int conferenceId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _conferenceService.ApproveConference(conferenceId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Conference đã được phê duyệt thành công" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("reject-conference/{conferenceId}")]
    [Authorize]
    public async Task<IActionResult> RejectConference(int conferenceId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _conferenceService.RejectConference(conferenceId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Conference đã bị từ chối" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }





[HttpGet("conference/list-all")]
[Authorize]
public async Task<IActionResult> GetAllConferences()
{
    try
    {
        var conferences = await _conferenceService.GetAllConferences();
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conference/{conferenceId}")]
[Authorize]
public async Task<IActionResult> GetConferenceById(int conferenceId)
{
    try
    {
        var conference = await _conferenceService.GetConferenceById(conferenceId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference retrieved successfully", conference));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conference/project/{projectId}")]
[Authorize]
public async Task<IActionResult> GetConferencesByProjectId(int projectId)
{
    try
    {
        var conferences = await _conferenceService.GetConferencesByProjectId(projectId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}
} 