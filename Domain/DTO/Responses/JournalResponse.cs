using System;

namespace Domain.DTO.Responses;

public class JournalResponse
{
    public int JournalId { get; set; }
    public string? JournalName { get; set; }
    public string? PublisherName { get; set; }
    public int? PublisherStatus { get; set; }
    public string? DoiNumber { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public string? ReviewerComments { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? DepartmentName { get; set; }
    public DocumentResponse? Document { get; set; }
} 