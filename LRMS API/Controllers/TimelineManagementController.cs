using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using System.Security.Claims;

namespace LRMS_API.Controllers;

[ApiController]
[Authorize]
public class TimelineManagementController : ApiBaseController
{
    private readonly ITimelineManagementService _timelineManagementService;

    public TimelineManagementController(ITimelineManagementService timelineManagementService)
    {
        _timelineManagementService = timelineManagementService;
    }

    [HttpGet("timelines/active")]
    public async Task<IActionResult> GetActiveTimelines()
    {
        try
        {
            var timelines = await _timelineManagementService.GetCurrentActiveTimelines();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Active timelines retrieved", timelines));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("timelines/create-action-timeline")]
    public async Task<IActionResult> CreateActionTimeline([FromBody] TimelineRequest request)
    {
        try 
        {
            if (request.TimelineType == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Timeline type is required"));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var timeline = await _timelineManagementService.CreateActionTimeline(request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Action timeline created", timeline));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPut("timelines/{timelineId}/extend")]
    public async Task<IActionResult> ExtendTimeline(int timelineId, [FromBody] ExtendTimelineRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _timelineManagementService.ExtendTimeline(timelineId, request.NewEndDate, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Timeline extended successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
} 