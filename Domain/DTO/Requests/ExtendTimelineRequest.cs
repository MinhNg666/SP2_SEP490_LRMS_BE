using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests
{
    public class ExtendTimelineRequest
    {
        [Required]
        public DateTime NewEndDate { get; set; }
    }
} 