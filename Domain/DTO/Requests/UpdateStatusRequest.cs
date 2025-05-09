using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests
{
    public class UpdateStatusRequest
    {
        [Required]
        public int? Status { get; set; }
    }
} 