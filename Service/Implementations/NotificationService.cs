using AutoMapper;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Domain.Constants;
using LRMS_API;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public NotificationService(
        INotificationRepository notificationRepository,
        IInvitationRepository invitationRepository,
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _invitationRepository = invitationRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task CreateNotification(CreateNotificationRequest request)
    {
        try
        {
            var notification = _mapper.Map<Notification>(request);
            notification.CreatedAt = DateTime.Now; // Set creation timestamp
            await _notificationRepository.AddAsync(notification);

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    request.Title,
                    request.Message
                );
            }
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
                notification.Status = 1;     
                notification.IsRead = true;  
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
        // Get the invitation details to include group info in the message
        var invitation = await _invitationRepository.GetInvitationById(invitationId);
        if (invitation == null)
        {
            throw new ServiceException("Invitation not found");
        }
        
        // Validate GroupId is not null
        if (!invitation.GroupId.HasValue)
        {
            throw new ServiceException("Group ID is missing from invitation");
        }
        
        // Get group info for the updated message
        var group = await _groupRepository.GetByIdAsync(invitation.GroupId.Value);
        var groupName = group?.GroupName ?? "the group"; 
        
        // Get sender info for the updated message - handle null SentBy
        string senderName = "the group creator"; 
        if (invitation.SentBy.HasValue)
        {
            var sender = await _userRepository.GetByIdAsync(invitation.SentBy.Value);
            if (sender != null)
            {
                senderName = sender.FullName ?? "the group creator";
            }
        }
        
        // Create new message if user accepted or rejected 
        string updatedMessage;
        if (newStatus == (int)InvitationEnum.Accepted)
        {
            updatedMessage = $"You have accepted an invitation from {senderName} to join '{groupName}'";
        }
        else if (newStatus == (int)InvitationEnum.Rejected)
        {
            updatedMessage = $"You have rejected an invitation from {senderName} to join '{groupName}'";
        }
        else
        {
            // Keep original message for other statuses
            updatedMessage = null;
        }
        
        var notifications = await _notificationRepository.GetByInvitationIdAsync(invitationId);
        if (notifications == null)
        {
            // If no notifications found for this invitation
            return;
        }
        
        foreach (var notification in notifications)
        {
            if (notification == null) continue;
            
            // Update notification status
            notification.Status = newStatus;
            // Mark as read if the invitation has been processed
            notification.IsRead = true;
            
            // Update the message 
            if (!string.IsNullOrEmpty(updatedMessage))
            {
                notification.Message = updatedMessage;
            }
            
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
