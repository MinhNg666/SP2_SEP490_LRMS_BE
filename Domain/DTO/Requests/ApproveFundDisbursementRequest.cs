namespace Domain.DTO.Requests;
public class ApproveFundDisbursementRequest
{
    public int FundDisbursementId { get; set; }
    public string? ApprovalComments { get; set; }
}
