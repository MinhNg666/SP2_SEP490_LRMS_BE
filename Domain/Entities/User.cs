using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class User
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? GroupId { get; set; }

    public string? Level { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Invitation> InvitationInvitedByNavigations { get; set; } = new List<Invitation>();

    public virtual ICollection<Invitation> InvitationInvitedUsers { get; set; } = new List<Invitation>();

    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> ProjectApprovedByNavigations { get; set; } = new List<Project>();

    public virtual ICollection<Project> ProjectUsers { get; set; } = new List<Project>();

    public virtual ICollection<Quota> Quota { get; set; } = new List<Quota>();

    public virtual ICollection<Timeline> Timelines { get; set; } = new List<Timeline>();
}
