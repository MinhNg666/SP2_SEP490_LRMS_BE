using System;
using Domain.Constants;

namespace Domain.DTO.Responses
{
    public class CompletionRequestResponse
    {
        public int RequestId { get; set; }
        public ProjectRequestTypeEnum RequestType { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public ProjectStatusEnum ProjectStatus { get; set; }
        public ApprovalStatusEnum? ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RejectionReason { get; set; }
        
        // Requester information
        public string RequesterName { get; set; }
        public int RequestedById { get; set; }
        
        // Approver information
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproverName { get; set; }
        
        // CompletionRequestDetail data
        public decimal? BudgetRemaining { get; set; }
        public bool? BudgetReconciled { get; set; }
        public string? CompletionSummary { get; set; }
        public string? BudgetVarianceExplanation { get; set; }
        
        // Additional helpful info
        public decimal? ApprovedBudget { get; set; }
        public decimal? SpentBudget { get; set; }
        
        // Group information
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        
        // Department information
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
