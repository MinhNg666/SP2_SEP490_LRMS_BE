using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class ProjectCompletionSummaryResponse
{
    // Project info
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public int? ProjectType { get; set; }
    public string ProjectTypeName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Budget information
    public decimal ApprovedBudget { get; set; }
    public decimal SpentBudget { get; set; }
    public decimal RemainingBudget => ApprovedBudget - SpentBudget;
    
    // Phase information
    public int TotalPhases { get; set; }
    public int CompletedPhases { get; set; }
    public ICollection<ProjectPhaseResponse> Phases { get; set; }
    
    // Documents
    public int DocumentCount { get; set; }
    public ICollection<DocumentResponse> Documents { get; set; }
    
    // Fund disbursements
    public decimal TotalDisbursedAmount { get; set; }
    public int DisbursementCount { get; set; }
    public ICollection<FundDisbursementResponse> Disbursements { get; set; }
    
    // Team members
    public ICollection<GroupMemberResponse> TeamMembers { get; set; }
}
