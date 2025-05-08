using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class ConferenceExpenseRequestResponse
{
    // Request information
    public int RequestId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public int RequestedById { get; set; }
    public string RequesterName { get; set; }
    public DateTime RequestedAt { get; set; }
    
    // Conference information
    public int ConferenceId { get; set; }
    public string ConferenceName { get; set; }
    
    // Fund disbursement information
    public int FundDisbursementId { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Expense details
    public string AccommodationDetails { get; set; }
    public decimal AccommodationAmount { get; set; }
    public string TravelDetails { get; set; }
    public decimal TravelAmount { get; set; }
    
    // Status information
    public int ExpenseStatus { get; set; }
    public string ExpenseStatusName { get; set; }

    // Add this property
    public int? FundDisbursementType { get; set; }
    public string? FundDisbursementTypeName { get; set; }
    
    // Add rejection reason
    public string? RejectionReason { get; set; }
} 