using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

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
            var result = await _timelineService.CreateTimeline(request);
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
} 