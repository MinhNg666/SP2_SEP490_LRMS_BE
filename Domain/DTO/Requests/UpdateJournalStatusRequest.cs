namespace Domain.DTO.Requests;

public class UpdateJournalStatusRequest
{
    public int PublisherStatus { get; set; }
    public string? ReviewerComments { get; set; }
} 