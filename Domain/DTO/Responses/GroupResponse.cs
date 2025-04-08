using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses;
public class GroupResponse
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public int MaxMember { get; set; }
    public int CurrentMember { get; set; }
    public int Status { get; set; }
    public int GroupType { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<GroupMemberResponse> Members { get; set; }
}
