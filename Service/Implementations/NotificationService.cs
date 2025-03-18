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
            var Notification = _mapper.Map<Notification>(request);
            await _notificationRepository.AddAsync(Notification);
        }
        catch (Exception ex)
        {
            throw new ServiceException(ex.Message);
        }
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId)
    {
        var notifications = await _notificationRepository.GetByIdAsync(userId);
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
}
