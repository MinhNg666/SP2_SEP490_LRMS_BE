namespace Domain.DTO.Requests;
using System.ComponentModel.DataAnnotations;
using Domain.Constants;

public class CreateConferenceFromProjectRequest
{
    [Required]
    public string ConferenceName { get; set; }
    
    [Required]
    [Range(0, 4, ErrorMessage = "Conference ranking must be between 0 (Not Ranked) and 4 (A*)")]
    public int ConferenceRanking { get; set; }
    
    [Required]
    [Range(0, 5, ErrorMessage = "Presentation type must be valid (0-5)")]
    public int PresentationType { get; set; }
    
}
