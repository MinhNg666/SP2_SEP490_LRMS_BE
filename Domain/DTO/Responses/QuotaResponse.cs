using System;

namespace Domain.DTO.Responses
{
    public class QuotaResponse
    {
        public int QuotaId { get; set; }
        public decimal? AllocatedBudget { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? AllocatedBy { get; set; }
        public string AllocatorName { get; set; }
    }
}
