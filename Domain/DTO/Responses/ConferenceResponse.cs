using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class ConferenceResponse
{
    public int ConferenceId { get; set; }
    public string ConferenceName { get; set; }
    public int ConferenceRanking { get; set; }
    public string Location { get; set; }
    public DateTime PresentationDate { get; set; }
    public DateTime AcceptanceDate { get; set; }
    public int PresentationType { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public ConferenceExpenseResponse Expense { get; set; }
    public List<DocumentResponse> Documents { get; set; }
}

public class ConferenceExpenseResponse
{
    public int ExpenseId { get; set; }
    public string Accomodation { get; set; }
    public decimal AccomodationExpense { get; set; }
    public string Travel { get; set; }
    public decimal TravelExpense { get; set; }
} 