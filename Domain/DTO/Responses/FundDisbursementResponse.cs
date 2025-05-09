using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;
public class FundDisbursementResponse
{
    public int FundDisbursementId { get; set; }
    public decimal FundRequest { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? QuotaId { get; set; }
    public int? ProjectPhaseId { get; set; }
    public string? ProjectPhaseTitle { get; set; }

   public int? FundDisbursementType { get; set; }
   public string? FundDisbursementTypeName { get; set; }
   
   
   public int? ConferenceId { get; set; }
   public string? ConferenceName { get; set; }
   public int? JournalId { get; set; }
   public string? JournalName { get; set; }
    
    // Requester Information
    public int RequesterId { get; set; }  
    public string? RequesterName { get; set; }  

    // Add Approver Information
    public int? ApprovedById { get; set; } 
    public string? ApprovedByName { get; set; } 

    // Add Disburser Information
    public int? DisbursedById { get; set; }
    public string? DisbursedByName { get; set; }
    
    public ICollection<DocumentResponse> Documents { get; set; } = new List<DocumentResponse>();

    public int? ProjectType { get; set; }
    public string? ProjectTypeName { get; set; }
    public decimal? ProjectApprovedBudget { get; set; }
    public decimal ProjectSpentBudget { get; set; } = 0;
    public decimal ProjectDisbursedAmount { get; set; } = 0;
    public ICollection<ProjectPhaseInfo>? ProjectPhases { get; set; }
    public string? RejectionReason { get; set; }
    public int? RequestId { get; set; }
    
    // Conference expense details 
    public ConferenceExpenseDetail? ConferenceExpenseDetail { get; set; }
    
    // Conference funding details
    public ConferenceFundingDetail? ConferenceFundingDetail { get; set; }
    
    // Journal funding details
    public JournalFundingDetail? JournalFundingDetail { get; set; }
}

public class ProjectPhaseInfo
{
    public int ProjectPhaseId { get; set; }
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Status { get; set; }
    public string? StatusName { get; set; }
    public decimal SpentBudget { get; set; } = 0;
}

// Added class to include detailed conference expense information
public class ConferenceExpenseDetail
{
    public int ExpenseId { get; set; }
    public string? Accommodation { get; set; }
    public decimal AccommodationExpense { get; set; }
    public string? Travel { get; set; }
    public decimal TravelExpense { get; set; }
    public int ExpenseStatus { get; set; }
    public string? ExpenseStatusName { get; set; }
    public string? RejectionReason { get; set; }
}

// Added class to include detailed conference funding information
public class ConferenceFundingDetail
{
    public string? Location { get; set; }
    public DateTime? PresentationDate { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public decimal? ConferenceFunding { get; set; }
}

// Added class to include detailed journal funding information
public class JournalFundingDetail
{
    public string? DoiNumber { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public decimal? JournalFunding { get; set; }
}
