using System;
using System.Text.Json.Serialization;

namespace Domain.DTO.Requests;

public class TimelineRequest
{
    public int? SequenceId { get; set; }
    
    [JsonConverter(typeof(JsonDateTimeConverter))]
    public DateTime? StartDate { get; set; }
    
    [JsonConverter(typeof(JsonDateTimeConverter))]
    public DateTime? EndDate { get; set; }
    
    public string? Event { get; set; }
    public int? TimelineType { get; set; }
    public int? CreatedBy { get; set; }
} 