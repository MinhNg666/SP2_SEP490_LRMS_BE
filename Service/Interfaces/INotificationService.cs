using Domain.DTO.Responses;

namespace Service.Interfaces;
public interface INotificationService
{
    //Task CreateNotification(int userId, string title, string message, int? projectId = null, int? invitationId = null);
    Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId);
    Task MarkAsRead(int notificationId);
}
