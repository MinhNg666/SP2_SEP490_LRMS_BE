using Microsoft.AspNetCore.Http;
namespace Domain.DTO.Requests;
public class CreateProjectRequest
{
    //Project Info
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string Methodology { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public int GroupId { get; set; }
    public int DepartmentId { get; set; }
    public List<MilestoneRequest> Milestones { get; set; }
    public int SequenceId { get; set; }
}
public class MilestoneRequest
{
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
