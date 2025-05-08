using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses
{
    public class QuotaDetailResponse : QuotaResponse
    {
        public ICollection<DisbursementInfo> Disbursements { get; set; } = new List<DisbursementInfo>();
    }

    public class DisbursementInfo
    {
        public int FundDisbursementId { get; set; }
        public decimal FundRequest { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Description { get; set; }
        
        // Requester Information
        public int RequesterId { get; set; }
        public string RequesterName { get; set; }
        
        // Approver Information
        public int? ApprovedById { get; set; }
        public string ApprovedByName { get; set; }
        
        // Project phase information (if applicable)
        public int? ProjectPhaseId { get; set; }
        public string ProjectPhaseTitle { get; set; }

        public int? FundDisbursementType { get; set; }
        public string FundDisbursementTypeName { get; set; }
    }
}
