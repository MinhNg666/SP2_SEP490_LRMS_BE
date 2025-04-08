using System;

namespace Domain.DTO.Responses;

public class TimelineSequenceResponse
{
    public int Id { get; set; }
    public string? SequenceName { get; set; }
    public string? SequenceDescription { get; set; }
    public string? SequenceColor { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
} 