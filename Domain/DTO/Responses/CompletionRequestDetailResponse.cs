using System;
using System.Collections.Generic;
using Domain.Constants;

namespace Domain.DTO.Responses
{
    public class CompletionRequestDetailResponse : CompletionRequestResponse
    {
        // Add documents related to this request
        public ICollection<DocumentResponse> Documents { get; set; }
        
        // Add project status information
        public ProjectStatusEnum ProjectStatus { get; set; }
        
        // Additional fields from CompletionRequestDetail
        public string ProjectDescription { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
    }
}
