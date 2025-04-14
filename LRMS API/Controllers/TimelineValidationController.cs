// using Domain.Constants;
// using Domain.DTO.Common;
// using Microsoft.AspNetCore.Mvc;
// using Service.Exceptions;
// using Service.Interfaces;

// namespace LRMS_API.Controllers;

// [ApiController]
// public class TimelineValidationController : ApiBaseController
// {
//     private readonly ITimelineService _timelineService;

//     public TimelineValidationController(ITimelineService timelineService)
//     {
//         _timelineService = timelineService;
//     }

//     [HttpGet("api/timeline-validation/{timelineType}")]
//     public async Task<IActionResult> ValidateTimeForAction(int timelineType, [FromQuery] int? sequenceId = null)
//     {
//         try
//         {
//             var isValid = await _timelineService.IsValidTimeForAction(timelineType, sequenceId);
//             if (!isValid)
//             {
//                 return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Out of time"));
//             }
//             return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
//         }
//         catch (ServiceException e)
//         {
//             return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
//         }
//     }
// } 