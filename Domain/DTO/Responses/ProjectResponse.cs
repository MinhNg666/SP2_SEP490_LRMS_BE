using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;

namespace Domain.DTO.Responses
{
    public class ProjectResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? ProjectType { get; set; }
        public string Description { get; set; }
        public decimal ApprovedBudget { get; set; }
        public decimal SpentBudget { get; set; }
        public int? Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Methodology { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DepartmentId { get; set; }
        public string? RejectionReason { get; set; }
        public ICollection<DocumentResponse> Documents { get; set; }
        public ICollection<ProjectPhaseResponse> ProjectPhases { get; set; }
    }
}
