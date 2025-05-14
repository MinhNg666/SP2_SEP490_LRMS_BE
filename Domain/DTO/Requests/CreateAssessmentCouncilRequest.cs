using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;

namespace Domain.DTO.Requests;
public class CreateAssessmentCouncilRequest
{
    public string GroupName { get; set; }
    public int ProjectId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public int? GroupDepartment { get; set; }
    public List<AssessmentMemberRequest> Members { get; set; }
}

public class AssessmentMemberRequest
{
    public string MemberEmail { get; set; }
    public string MemberName { get; set; }
    public int Role { get; set; }
}
