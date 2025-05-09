using System;
using System.Collections.Generic;

namespace Domain.DTO.Responses;

public class JournalDetailResponse
{
    public int JournalId { get; set; }
    public string JournalName { get; set; }
    public string PublisherName { get; set; }
    public int PublisherStatus { get; set; }
    public string PublisherStatusName { get; set; }
    public string DoiNumber { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public string ReviewerComments { get; set; }
    public decimal? JournalFunding { get; set; }
    public int? JournalStatus { get; set; }
    
    // Project information
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public int ProjectStatus { get; set; }
    
    // Related documents
    public List<DocumentResponse> Documents { get; set; } = new List<DocumentResponse>();
} 