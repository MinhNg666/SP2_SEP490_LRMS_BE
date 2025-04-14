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
public class TimelineController : ApiBaseController
{
    private readonly ITimelineService _timelineService;

    public TimelineController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    [HttpPost("timelines")]
    public async Task<IActionResult> CreateTimeline([FromBody] TimelineRequest request)
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
            
            var result = await _timelineService.CreateTimeline(request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("timelines")]
    public async Task<IActionResult> GetAllTimelines()
    {
        try
        {
            var result = await _timelineService.GetAllTimelines();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("timelines/{id}")]
    public async Task<IActionResult> GetTimelineById(int id)
    {
        try
        {
            var result = await _timelineService.GetTimelineById(id);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpGet("timelines/by-sequence/{sequenceId}")]
    public async Task<IActionResult> GetTimelinesBySequenceId(int sequenceId)
    {
        try
        {
            var result = await _timelineService.GetTimelinesBySequenceId(sequenceId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPut("timelines/{id}")]
    public async Task<IActionResult> UpdateTimeline(int id, [FromBody] TimelineRequest request)
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
            
            var result = await _timelineService.UpdateTimeline(id, request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
} 