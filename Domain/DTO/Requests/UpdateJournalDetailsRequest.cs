using System;

namespace Domain.DTO.Requests;

public class UpdateJournalDetailsRequest
{
    public string DoiNumber { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public decimal? JournalFunding { get; set; }
} 