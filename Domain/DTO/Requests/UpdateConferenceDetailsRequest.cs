using System;
using System.ComponentModel.DataAnnotations;

public class UpdateConferenceDetailsRequest
{
    [Required]
    public int Status { get; set; } // Use ConferenceStatusEnum values
    
    [Required]
    public DateTime PresentationDate { get; set; }
    
    [Required]
    public DateTime AcceptanceDate { get; set; }
    
    public string Location { get; set; }
    
    // Optional additional expense information
    public ConferenceExpenseRequest Expenses { get; set; }
}

public class ConferenceExpenseRequest
{
    public string Accommodation { get; set; }
    public decimal AccommodationExpense { get; set; }
    public string Travel { get; set; }
    public decimal TravelExpense { get; set; }
}
