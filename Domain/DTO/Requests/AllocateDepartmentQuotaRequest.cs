using System;

namespace Domain.DTO.Requests
{
    public class AllocateDepartmentQuotaRequest
    {
        public int DepartmentId { get; set; }
        public decimal AllocatedBudget { get; set; }
        public int? QuotaYear { get; set; }
        public int? NumProjects { get; set; }
    }
} 