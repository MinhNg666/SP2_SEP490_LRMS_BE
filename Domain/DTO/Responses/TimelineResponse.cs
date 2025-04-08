using System;

namespace Domain.DTO.Responses;

public class TimelineResponse
{
    public int Id { get; set; }
    public int? SequenceId { get; set; }
    public string? SequenceName { get; set; }
    public string? SequenceColor { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Event { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public int? TimelineType { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
} 