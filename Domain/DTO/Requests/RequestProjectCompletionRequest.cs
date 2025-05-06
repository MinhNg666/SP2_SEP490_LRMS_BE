using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTO.Requests
{
    public class RequestProjectCompletionRequest
    {
        [Required]
        public string CompletionSummary { get; set; }

        [Required]
        // Ensure the frontend sends this as 'true' explicitly for confirmation
        public bool BudgetReconciled { get; set; }

        // Optional explanation if budget variance is significant
        public string? BudgetVarianceExplanation { get; set; }

        // Note: Files will be handled separately in the controller action
    }
}
