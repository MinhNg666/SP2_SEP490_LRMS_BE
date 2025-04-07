using AutoMapper;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
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

    public async Task CreateNotification(CreateNotificationRequest request)
    {
        try 
        { 
            var notification = _mapper.Map<Notification>(request);
            notification.CreatedAt = DateTime.Now; // Set creation timestamp
            await _notificationRepository.AddAsync(notification);
        }
        catch (Exception ex)
        {
            throw new ServiceException(ex.Message);
        }
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
    }

    public async Task MarkAsRead(int notificationId)
    {
        try 
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.Status = 1; // 1: Read
                await _notificationRepository.UpdateAsync(notification);
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException(ex.Message);
        }
        
    }

    public async Task UpdateNotificationForInvitation(int invitationId, int newStatus)
    {
        var notifications = await _notificationRepository.GetByInvitationIdAsync(invitationId);
        
        foreach (var notification in notifications)
        {
            // Update notification to reflect invitation status
            notification.Status = newStatus;
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
