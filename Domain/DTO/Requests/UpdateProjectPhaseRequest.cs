namespace Domain.DTO.Requests
{
    public class UpdateProjectPhaseRequest
    {
        public int ProjectPhaseId { get; set; }
        public int Status { get; set; }
        public decimal SpentBudget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Title { get; set; }
    }
}
