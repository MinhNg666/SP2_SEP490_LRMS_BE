using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Group
{
    public int GroupId { get; set; }

    public int? GroupType { get; set; }

    public string? GroupName { get; set; }

    public int? MaxMember { get; set; }

    public int? CurrentMember { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? GroupDepartment { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Department? GroupDepartmentNavigation { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
