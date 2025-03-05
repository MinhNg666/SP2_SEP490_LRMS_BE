using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class GroupMember
{
    public int GroupMemberId { get; set; }

    public int? GroupId { get; set; }

    public string? MemberName { get; set; }

    public string? MemberEmail { get; set; }

    public int? Role { get; set; }

    public int? UserId { get; set; }

    public int? Status { get; set; }

    public DateTime? JoinDate { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? User { get; set; }
}
