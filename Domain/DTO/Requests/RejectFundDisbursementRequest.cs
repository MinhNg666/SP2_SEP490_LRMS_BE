namespace Domain.DTO.Requests;
public class RejectFundDisbursementRequest
{
    public int FundDisbursementId { get; set; }
    public string RejectionReason { get; set; }
}
