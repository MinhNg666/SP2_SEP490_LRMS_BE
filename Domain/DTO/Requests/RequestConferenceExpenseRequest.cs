using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests;

public class RequestConferenceExpenseRequest
{
    [Required]
    public int ConferenceId { get; set; }
    
    [Required]
    public string Accommodation { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal AccommodationExpense { get; set; }
    
    [Required]
    public string Travel { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal TravelExpense { get; set; }
}
