namespace Domain.DTO.Requests;
public class CreateFundDisbursementRequest
{
    public decimal FundRequest { get; set; }
    public string Description { get; set; }
    public int ProjectId { get; set; }
    public int? ProjectPhaseId { get; set; } // Optional, link to specific project phase
}
