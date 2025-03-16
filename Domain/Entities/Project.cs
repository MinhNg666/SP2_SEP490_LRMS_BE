using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Project
{
    public int ProjectId { get; set; }

    public string? ProjectName { get; set; }

    public int? ProjectType { get; set; }

    public string? Description { get; set; }

    public decimal? ApprovedBudget { get; set; }

    public decimal SpentBudget { get; set; }

    public int? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Methodlogy { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public int? CreatedBy { get; set; }

    public int? GroupId { get; set; }

    public int? DepartmentId { get; set; }

    public virtual GroupMember? ApprovedByNavigation { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    public virtual Group? Group { get; set; }

    public virtual ICollection<Journal> Journals { get; set; } = new List<Journal>();

    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ProjectResource> ProjectResources { get; set; } = new List<ProjectResource>();

    public virtual ICollection<Quota> Quota { get; set; } = new List<Quota>();
}
