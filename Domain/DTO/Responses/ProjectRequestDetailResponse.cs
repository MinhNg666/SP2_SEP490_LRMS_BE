using System;
using System.Collections.Generic;
using Domain.Constants;

namespace Domain.DTO.Responses
{
    public class ProjectRequestDetailResponse
    {
        // Request information from ProjectRequestResponse
        public int RequestId { get; set; }
        public ProjectRequestTypeEnum RequestType { get; set; }
        public ApprovalStatusEnum? ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? StatusName { get; set; }
        
        // Requester & Approver information
        public UserShortInfo RequestedBy { get; set; }
        public UserShortInfo ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Fund disbursement information
        public int? FundDisbursementId { get; set; }
        public decimal? FundRequestAmount { get; set; }
        
        // Detailed project information (from ProjectDetailResponse)
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? ProjectType { get; set; }
        public string? ProjectTypeName { get; set; }
        public string Description { get; set; }
        public decimal? ApprovedBudget { get; set; }
        public decimal SpentBudget { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Methodology { get; set; }
        
        // Group & Department information
        public GroupDetailInfo Group { get; set; }
        public DepartmentResponse Department { get; set; }
        
        // Project phases & documents
        public ICollection<ProjectPhaseResponse> ProjectPhases { get; set; }
        public ICollection<DocumentResponse> Documents { get; set; }
        
        // Add these completion-specific properties
        public decimal? BudgetRemaining { get; set; }
        public bool? BudgetReconciled { get; set; }
        public string? CompletionSummary { get; set; }
        public string? BudgetVarianceExplanation { get; set; }

        // Add these properties for Fund Disbursement Type
        public int? FundDisbursementType { get; set; }
        public string? FundDisbursementTypeName { get; set; }
        public int? ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public int? JournalId { get; set; }
        public string? JournalName { get; set; }
    }
}
