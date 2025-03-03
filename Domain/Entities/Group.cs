using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Group
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public int? MaxMember { get; set; }

    public int? CurrentMember { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? GroupType { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
