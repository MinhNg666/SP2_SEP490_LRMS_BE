using Microsoft.AspNetCore.Http;
using System;

namespace Domain.DTO.Requests;

public class CreateConferenceRequest
{
    // Conference Info
    public string ConferenceName { get; set; }
    public int ConferenceRanking { get; set; }
    public string Location { get; set; }
    public DateTime PresentationDate { get; set; }
    public DateTime AcceptanceDate { get; set; }
    public int PresentationType { get; set; }
    public int ProjectId { get; set; }
    
    // Conference Expense Info
    public string Accomodation { get; set; }
    public decimal AccomodationExpense { get; set; }
    public string Travel { get; set; }
    public decimal TravelExpense { get; set; }
} 