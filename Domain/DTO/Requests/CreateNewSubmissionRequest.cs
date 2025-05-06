using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests;

public class CreateNewSubmissionRequest
{
    [Required]
    public string ConferenceName { get; set; }
    
    [Required]
    [Range(0, 4)]
    public int ConferenceRanking { get; set; }
    
    public string ReviewerComment { get; set; }
}