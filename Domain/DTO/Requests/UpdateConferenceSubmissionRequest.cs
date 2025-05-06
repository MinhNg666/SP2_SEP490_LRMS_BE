using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests;

public class UpdateConferenceSubmissionRequest
{
    [Required]
    public int SubmissionStatus { get; set; } // Use ConferenceSubmissionStatusEnum

    public string? ReviewerComment { get; set; }
    
    // Fields to update when submission is approved
    public string? Location { get; set; }
    public DateTime? PresentationDate { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public decimal? ConferenceFunding { get; set; }
    
    // Only required when submission is rejected and creating a new submission
    public string? NewConferenceName { get; set; }
    public int? NewConferenceRanking { get; set; }
}
