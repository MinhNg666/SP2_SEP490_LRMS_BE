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
        
        // Project information
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal? ProjectApprovedBudget { get; set; }
        public decimal ProjectSpentBudget { get; set; }
        
        // Department information
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        
        // Group information
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        
        // Allocator information
        public int? AllocatedBy { get; set; }
        public string AllocatorName { get; set; }
        
        // Project type information
        public int? ProjectType { get; set; }
        public string ProjectTypeName { get; set; }
        
        public decimal DisbursedAmount { get; set; }
    }
}
