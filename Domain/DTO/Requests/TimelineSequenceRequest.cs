using System;

namespace Domain.DTO.Requests;

public class TimelineSequenceRequest
{
    public string? SequenceName { get; set; }
    public string? SequenceDescription { get; set; }
    public string? SequenceColor { get; set; }
    public int? Status { get; set; }
} 