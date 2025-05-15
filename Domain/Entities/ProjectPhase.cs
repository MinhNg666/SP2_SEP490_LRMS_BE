using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class ProjectPhase
{
    public int ProjectPhaseId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Status { get; set; }
    public int? AssignTo { get; set; }
    public int? AssignBy { get; set; }
    public int? ProjectId { get; set; }
    public decimal SpentBudget { get; set; } = 0;

    public virtual GroupMember? AssignByNavigation { get; set; }
    public virtual GroupMember? AssignToNavigation { get; set; }
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual Project? Project { get; set; }

    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    // Navigation property to ProjectResources
    public virtual ICollection<ProjectResource> ProjectResources { get; set; } = new List<ProjectResource>();

    // Navigation property to ProgressReports (1-N)
    public virtual ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
}
