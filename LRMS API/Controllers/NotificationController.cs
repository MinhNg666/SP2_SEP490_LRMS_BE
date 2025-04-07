using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace LRMS_API.Controllers;
[ApiController]
[Route("api/notifications")]
public class NotificationController : ApiBaseController
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int userId)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByUserId(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, notifications));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification(CreateNotificationRequest request)
    {
        try
        {
            await _notificationService.CreateNotification(request);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL,"Notification created"));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }

    [HttpPatch("{notificationId}")]
    public async Task<IActionResult> UpdateNotification(int notificationId, [FromBody] UpdateNotificationRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request body is required"));
            }
            
            if (request.IsRead.HasValue && request.IsRead.Value)
            {
                await _notificationService.MarkAsRead(notificationId);
            }
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, "Notification updated"));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
