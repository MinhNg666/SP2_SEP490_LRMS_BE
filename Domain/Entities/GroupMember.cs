using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class GroupMember
{
    public int GroupMemberId { get; set; }

    public int? Role { get; set; }

    public DateTime? JoinDate { get; set; }

    public int? Status { get; set; }

    public int? UserId { get; set; }

    public int? GroupId { get; set; }

    public virtual ICollection<FundDisbursement> FundDisbursementAppovedByNavigations { get; set; } = new List<FundDisbursement>();

    public virtual ICollection<FundDisbursement> FundDisbursementSupervisorRequestNavigations { get; set; } = new List<FundDisbursement>();

    public virtual Group? Group { get; set; }

    public virtual ICollection<Milestone> MilestoneAssignByNavigations { get; set; } = new List<Milestone>();

    public virtual ICollection<Milestone> MilestoneAssignToNavigations { get; set; } = new List<Milestone>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual User? User { get; set; }
}
