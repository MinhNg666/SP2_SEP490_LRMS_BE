using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;

public class CreateStudentGroupRequest
{
    public string GroupName { get; set; }
    public int MaxMember { get; set; }
    public int? CurrentMember { get; set; }
    public int? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? GroupType { get; set; }
    public List<StudentMemberRequest> Members { get; set; }

}
public class StudentMemberRequest 
{
    public string MemberName { get; set; }
    public string MemberEmail { get; set; }
    public int Role { get; set; }
}
