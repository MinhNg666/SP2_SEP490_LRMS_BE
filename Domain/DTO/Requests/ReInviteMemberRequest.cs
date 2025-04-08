namespace Domain.DTO.Requests;

public class ReInviteMemberRequest
{
    public int GroupId { get; set; }
    public string MemberEmail { get; set; }
    public string MemberName { get; set; }
    public int Role { get; set; }
    public string? Message { get; set; } // Optional custom message
}
