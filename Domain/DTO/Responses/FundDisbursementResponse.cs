using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;
public class FundDisbursementResponse
{
    public int FundDisbursementId { get; set; }
    public decimal FundRequest { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string Description { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public int? QuotaId { get; set; }
    public int? ProjectPhaseId { get; set; }
    public string ProjectPhaseTitle { get; set; }
    public int AuthorRequest { get; set; }
    public string AuthorName { get; set; }
    public int SupervisorRequest { get; set; }
    public string SupervisorName { get; set; }
    public ICollection<DocumentResponse> Documents { get; set; } = new List<DocumentResponse>();
}
