using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses;
public class ResearcherResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string Level { get; set; }
} 