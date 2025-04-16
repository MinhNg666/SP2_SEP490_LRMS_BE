namespace Domain.DTO.Requests;
public class StakeholderEmailRequest
{
    public int GroupId { get; set; }
    public List<string> StakeholderEmails { get; set; }
}

