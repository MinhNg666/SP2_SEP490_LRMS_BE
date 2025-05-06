using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests;

public class UpdateApprovedConferenceRequest
{
    public string Location { get; set; }
    public DateTime PresentationDate { get; set; }
    public DateTime AcceptanceDate { get; set; }
    public decimal? ConferenceFunding { get; set; }
}