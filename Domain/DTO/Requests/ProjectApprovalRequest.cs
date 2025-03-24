namespace Domain.DTO.Requests;
public class ProjectApprovalRequest
{
    public int ProjectId { get; set; }
    public int CouncilGroupId { get; set; }
    public string? Comment { get; set; }
}
