using System;

namespace Domain.DTO.Requests;

public class TimelineRequest
{
    public int? SequenceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Event { get; set; }
    public int? TimelineType { get; set; }
    public int? CreatedBy { get; set; }
} 