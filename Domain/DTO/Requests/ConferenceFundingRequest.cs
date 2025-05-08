using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Domain.DTO.Requests;

public class RequestConferenceFundingRequest
{
    [Required]
    public int ConferenceId { get; set; }
    
    [Required]
    public string Location { get; set; }
    
    [Required]
    public DateTime PresentationDate { get; set; }
    
    [Required]
    public DateTime AcceptanceDate { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal ConferenceFunding { get; set; }
}
