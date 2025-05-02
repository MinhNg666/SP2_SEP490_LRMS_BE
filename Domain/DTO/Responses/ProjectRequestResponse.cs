using System;
using Domain.Constants;

namespace Domain.DTO.Responses
{
    public class ProjectRequestResponse
    {
        // Request information
        public int RequestId { get; set; }
        public ProjectRequestTypeEnum RequestType { get; set; }
        public ApprovalStatusEnum? ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RejectionReason { get; set; }
        
        // Requester information
        public int RequestedById { get; set; }
        public string RequesterName { get; set; }
        
        // Approver information (if available)
        public int? ApprovedById { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Project information
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public ProjectStatusEnum ProjectStatus { get; set; }
        public decimal? ApprovedBudget { get; set; }
        public decimal SpentBudget { get; set; }
        
        // Department information
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        
        // Group information
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        
        // Status name
        public string? StatusName { get; set; }

        // Fund disbursement information
        public int? FundDisbursementId { get; set; }
        public decimal? FundRequestAmount { get; set; }

        // Project type information
        public int? ProjectType { get; set; }
        public string? ProjectTypeName { get; set; }

        // Completion request specific fields
        public decimal? BudgetRemaining { get; set; }
        public bool? BudgetReconciled { get; set; }
        public string? CompletionSummary { get; set; }
        public string? BudgetVarianceExplanation { get; set; }
    }
}
