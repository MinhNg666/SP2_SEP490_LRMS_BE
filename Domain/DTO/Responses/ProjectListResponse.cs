namespace Domain.DTO.Responses
{
    public class ProjectListResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public double ApprovedBudget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Creator info
        public int CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        
        // Group info
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        
        // Department info
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}
