using Microsoft.AspNetCore.Http;
namespace Domain.DTO.Requests;
public class CreateProjectRequest
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string Methodology { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? ApprovedBudget { get; set; }
    // public int? Status { get; set; }
    public int GroupId { get; set; }
    public int DepartmentId { get; set; }
}
