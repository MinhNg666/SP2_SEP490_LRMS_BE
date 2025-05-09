using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Domain.DTO.Requests;

public class RequestJournalFundingRequest
{
    [Required]
    public int JournalId { get; set; }
    
    [Required]
    public string DoiNumber { get; set; }
    
    [Required]
    public DateTime AcceptanceDate { get; set; }
    
    [Required]
    public DateTime PublicationDate { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal JournalFunding { get; set; }
} 