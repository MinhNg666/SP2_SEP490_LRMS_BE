using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses;

public class GroupMemberResponse
{
    public int GroupMemberId { get; set; }
    public string MemberName { get; set; }
    public string MemberEmail { get; set; }
    public int Role { get; set; }
    public int? Status { get; set; }
    public DateTime JoinDate { get; set; }
}
