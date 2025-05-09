using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using System.Security.Claims;

namespace LRMS_API.Controllers;

[ApiController]
public class TimelineSequenceController : ApiBaseController
{
    private readonly ITimelineSequenceService _timelineSequenceService;

    public TimelineSequenceController(ITimelineSequenceService timelineSequenceService)
    {
        _timelineSequenceService = timelineSequenceService;
    }

    [HttpPost("timeline-sequences")]
    public async Task<IActionResult> CreateTimelineSequence([FromBody] TimelineSequenceRequest request)
    {
        try
        {
            // Get current user ID from claims
            int userId = User.Identity.IsAuthenticated 
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") 
                : 0;
            
            if (userId == 0)
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated"));
            }
            
            var result = await _timelineSequenceService.CreateTimelineSequence(request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("timeline-sequences")]
    public async Task<IActionResult> GetAllTimelineSequences()
    {
        try
        {
            var result = await _timelineSequenceService.GetAllTimelineSequences();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("timeline-sequences/{id}")]
    public async Task<IActionResult> GetTimelineSequenceById(int id)
    {
        try
        {
            var result = await _timelineSequenceService.GetTimelineSequenceById(id);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPut("timeline-sequences/{id}")]
    public async Task<IActionResult> UpdateTimelineSequence(int id, [FromBody] TimelineSequenceRequest request)
    {
        try
        {
            // Get current user ID from claims
            int userId = User.Identity.IsAuthenticated 
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") 
                : 0;
            
            if (userId == 0)
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated"));
            }
            
            var result = await _timelineSequenceService.UpdateTimelineSequence(id, request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Timeline sequence updated successfully", result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpDelete("timeline-sequences/{id}")]
    public async Task<IActionResult> DeleteTimelineSequence(int id)
    {
        try
        {
            var result = await _timelineSequenceService.DeleteTimelineSequence(id);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Timeline sequence deleted successfully", result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPut("timeline-sequences/{id}/update-timelines-status")]
    public async Task<IActionResult> UpdateTimelineStatusesInSequence(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            // Get current user ID from claims
            int userId = User.Identity.IsAuthenticated 
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") 
                : 0;
            
            if (userId == 0)
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated"));
            }
            
            if (!request.Status.HasValue)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Status is required"));
            }
            
            var result = await _timelineSequenceService.UpdateTimelineStatusesInSequence(id, request.Status.Value, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Timeline statuses updated successfully", result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
} 