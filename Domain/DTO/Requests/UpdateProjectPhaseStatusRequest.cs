using System;

namespace Domain.DTO.Requests
{
    public class UpdateProjectPhaseStatusRequest
    {
        public int ProjectPhaseId { get; set; }
        public int Status { get; set; }
    }
}
