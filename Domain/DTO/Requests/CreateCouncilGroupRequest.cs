using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;

public class CreateCouncilGroupRequest
{
    public string GroupName { get; set; }
    public List<MemberRequest> Members { get; set; }
}
public class MemberRequest
{
    public string MemberName { get; set; }
    public string MemberEmail { get; set; }
    public int Role { get; set; }
}
