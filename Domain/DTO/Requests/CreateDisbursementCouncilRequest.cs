using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;
public class CreateDisbursementCouncilRequest
{
    public string GroupName { get; set; }
    public int? GroupDepartment { get; set; }
    public int? ProjectId { get; set; }
    public List<DisbursementMemberRequest> Members { get; set; }
}

public class DisbursementMemberRequest
{
    public string MemberEmail { get; set; }
    public string MemberName { get; set; }
    public int Role { get; set; }
}