using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

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
            var result = await _timelineSequenceService.CreateTimelineSequence(request);
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
} 