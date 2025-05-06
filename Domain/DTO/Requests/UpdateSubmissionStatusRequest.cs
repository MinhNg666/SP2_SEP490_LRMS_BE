using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests;

public class UpdateSubmissionStatusRequest
{
    [Required]
    public int SubmissionStatus { get; set; }
    public string ReviewerComment { get; set; }
}
