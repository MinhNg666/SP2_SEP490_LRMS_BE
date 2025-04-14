using System;
using System.Text.Json.Serialization;
using Domain.Common;

namespace Domain.DTO.Responses;

public class TimelineResponse
{
    public int Id { get; set; }
    public int? SequenceId { get; set; }
    public string? SequenceName { get; set; }
    public string? SequenceColor { get; set; }
    
    [JsonConverter(typeof(JsonDateOnlyConverter))]
    public DateTime? StartDate { get; set; }
    
    [JsonConverter(typeof(JsonDateOnlyConverter))]
    public DateTime? EndDate { get; set; }
    
    public string? Event { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public int? TimelineType { get; set; }
    public int? Status { get; set; }
    public string? StatusName { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
} 