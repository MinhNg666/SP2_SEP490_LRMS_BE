using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace LRMS_API.Controllers;
[ApiController]

public class NotificationController : ApiBaseController
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotificationsByUserId(int userId)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByUserId(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("notification")]
    public async Task<IActionResult> CreateNotification( CreateNotificationRequest request)
    {
        try
        {
            await _notificationService.CreateNotification(request);
            return Ok("Notification created");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            await _notificationService.MarkAsRead(notificationId);
            return Ok("Notification marked as read");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
