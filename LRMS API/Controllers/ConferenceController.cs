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
using System.ComponentModel.DataAnnotations;

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
public async Task<IActionResult> CreateFromResearch(int projectId, [FromBody] CreateConferenceFromProjectRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conferenceId = await _conferenceService.CreateConferenceFromResearch(projectId, userId, request);
        return Ok(new { success = true, conferenceId = conferenceId, message = "Conference registration submitted successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("conferences/{conferenceId}/upload-documents")]
[Authorize]
public async Task<IActionResult> UploadConferenceDocuments(int conferenceId, [FromForm] List<IFormFile> documentFiles)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.AddConferenceDocuments(conferenceId, documentFiles, userId);
        return Ok(new { success = true, message = "Documents uploaded successfully" });
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

[HttpPut("conferences/{conferenceId}/update-submission")]
[Authorize]
public async Task<IActionResult> UpdateConferenceSubmission(int conferenceId, [FromBody] UpdateConferenceSubmissionRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateConferenceSubmission(conferenceId, userId, request);
        return Ok(new { success = true, message = "Conference submission updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPut("conferences/{conferenceId}/update-status")]
[Authorize]
public async Task<IActionResult> UpdateConferenceStatus(int conferenceId, [FromBody] int status)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateConferenceStatus(conferenceId, userId, status);
        return Ok(new { success = true, message = "Conference status updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}
[HttpGet("my-conferences")]
[Authorize]
public async Task<IActionResult> GetMyConferences()
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conferences = await _conferenceService.GetUserConferences(userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "User conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conferences/{conferenceId}/details")]
[Authorize]
public async Task<IActionResult> GetConferenceDetails(int conferenceId)
{
    try
    {
        var details = await _conferenceService.GetConferenceDetails(conferenceId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference details retrieved successfully", details));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPut("conferences/{conferenceId}/submission-status")]
[Authorize]
public async Task<IActionResult> UpdateSubmissionStatus(int conferenceId, [FromBody] UpdateSubmissionStatusRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateSubmissionStatus(conferenceId, userId, request.SubmissionStatus, request.ReviewerComment);
        return Ok(new { success = true, message = "Conference submission status updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPut("conferences/{conferenceId}/approved-details")]
[Authorize]
public async Task<IActionResult> UpdateApprovedConferenceDetails(int conferenceId, [FromBody] UpdateApprovedConferenceRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateApprovedConferenceDetails(conferenceId, userId, request);
        return Ok(new { success = true, message = "Approved conference details updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("projects/{projectId}/conferences/{rejectedConferenceId}/new-submission")]
[Authorize]
public async Task<IActionResult> CreateNewSubmissionAfterRejection(
    int projectId, 
    int rejectedConferenceId, 
    [FromBody] CreateNewSubmissionRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var newConferenceId = await _conferenceService.CreateNewSubmissionAfterRejection(
            projectId, userId, rejectedConferenceId, request);
        return Ok(new { 
            success = true, 
            message = "New conference submission created successfully", 
            conferenceId = newConferenceId 
        });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("conferences/{conferenceId}/request-expense")]
[Authorize]
public async Task<IActionResult> RequestConferenceExpense([FromBody] RequestConferenceExpenseRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var requestId = await _conferenceService.RequestConferenceExpenseAsync(userId, request);
        return Ok(new { 
            success = true, 
            message = "Conference expense request submitted successfully", 
            requestId = requestId 
        });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

} 