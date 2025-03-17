using AutoMapper;
using Domain.DTO.Responses;
using LRMS_API;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    //public async Task CreateNotification(int userId, string title, string message, int? projectId = null, int? invitationId = null)
    //{
    //    var notification = new Notification
    //    {
    //        UserId = userId,
    //        Title = title,
    //        Message = message,
    //        Status = 0, // 0: Unread
    //        CreatedAt = DateTime.Now,
    //        ProjectId = projectId,
    //        InvitationId = invitationId
    //    };

    //    await _notificationRepository.AddAsync(notification);
    //}

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId)
    {
        var notifications = await _notificationRepository.GetByIdAsync(userId);
        return _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
    }

    public async Task MarkAsRead(int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.Status = 1; // 1: Read
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
