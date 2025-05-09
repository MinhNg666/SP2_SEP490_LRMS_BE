using Domain.Constants;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

namespace LRMS_API.Controllers;

[ApiController]
public class TimelineValidationController : ApiBaseController
{
    private readonly ITimelineValidationService _timelineValidationService;

    public TimelineValidationController(ITimelineValidationService timelineValidationService)
    {
        _timelineValidationService = timelineValidationService;
    }

    [HttpGet("api/timeline-validation/{timelineType}")]
    public async Task<IActionResult> ValidateTimeForAction(int timelineType, [FromQuery] int? sequenceId = null)
    {
        try
        {
            var isValid = await _timelineValidationService.IsValidTimeForAction((TimelineTypeEnum)timelineType, sequenceId);
            if (!isValid)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Out of time period"));
            }
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
} 