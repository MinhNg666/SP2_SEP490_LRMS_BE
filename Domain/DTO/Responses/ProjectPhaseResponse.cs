using System;

namespace Domain.DTO.Responses
{
    public class ProjectPhaseResponse
    {
        public int ProjectPhaseId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public decimal SpentBudget { get; set; }
    }
}
