namespace Domain.DTO.Responses;

public class NotificationResponse
{
    public int NotificationId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public int? UserId { get; set; }
    public int? ProjectId { get; set; }
    public int? InvitationId { get; set; }
}
