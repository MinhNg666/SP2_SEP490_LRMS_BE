using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;
public interface INotificationService
{
    Task CreateNotification(CreateNotificationRequest request);
    Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId);
    Task MarkAsRead(int notificationId);
    Task UpdateNotificationForInvitation(int invitationId, int newStatus);
}
