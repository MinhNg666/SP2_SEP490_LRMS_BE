using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class ConferenceDetailResponse
{
    public int ConferenceId { get; set; }
    public string ConferenceName { get; set; }
    public int ConferenceRanking { get; set; }
    public string ConferenceRankingName { get; set; }
    public string Location { get; set; }
    public DateTime? PresentationDate { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public int PresentationType { get; set; }
    public string PresentationTypeName { get; set; }
    
    // Status information
    public int ConferenceStatus { get; set; }
    public string ConferenceStatusName { get; set; }
    public int ConferenceSubmissionStatus { get; set; }
    public string ConferenceSubmissionStatusName { get; set; }
    public decimal? ConferenceFunding { get; set; }
    public string ReviewerComment { get; set; }
    
    // Project information
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public int ProjectStatus { get; set; }
    
    // Expense information
    public ConferenceExpenseResponse Expense { get; set; }
    
    // Documents
    public List<DocumentResponse> Documents { get; set; }
}