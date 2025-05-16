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

    public int? Role { get; set; }

    public int? Level { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? DepartmentId { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Invitation> InvitationRecieveByNavigations { get; set; } = new List<Invitation>();

    public virtual ICollection<Invitation> InvitationSentByNavigations { get; set; } = new List<Invitation>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Quota> Quota { get; set; } = new List<Quota>();

    public virtual ICollection<Timeline> Timelines { get; set; } = new List<Timeline>();

    public virtual ICollection<FundDisbursement> FundDisbursementsAsRequester { get; set; } = new List<FundDisbursement>();

    // Navigation to AssignReviews (where User is AssignedBy)
    public virtual ICollection<AssignReview> AssignReviewsMade { get; set; } = new List<AssignReview>();

    // Navigation to Expertises (1-N)
    public virtual ICollection<Expertise> Expertises { get; set; } = new List<Expertise>();

    // Navigation to ProjectRequests (where User is RequestedBy)
    public virtual ICollection<ProjectRequest> ProjectRequestsMade { get; set; } = new List<ProjectRequest>();

    // Navigation to ProjectRequests (where User is ApprovedBy)
    public virtual ICollection<ProjectRequest> ProjectRequestsApproved { get; set; } = new List<ProjectRequest>();
}
