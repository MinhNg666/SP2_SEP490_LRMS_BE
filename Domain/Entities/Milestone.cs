using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Milestone
{
    public int MilestoneId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Status { get; set; }

    public int? AssignTo { get; set; }

    public int? AssignBy { get; set; }

    public int? ProjectId { get; set; }

    public virtual GroupMember? AssignByNavigation { get; set; }

    public virtual GroupMember? AssignToNavigation { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Project? Project { get; set; }
}
