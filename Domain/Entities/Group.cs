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

    // Navigation to CouncilVotes (1-N)
    public virtual ICollection<CouncilVote> CouncilVotes { get; set; } = new List<CouncilVote>();

    // Navigation to VoteResults (1-N)
    public virtual ICollection<VoteResult> VoteResults { get; set; } = new List<VoteResult>();

    // Navigation to AssignReviews (1-N)
    public virtual ICollection<AssignReview> AssignReviews { get; set; } = new List<AssignReview>();

    // Navigation to ProjectRequests (where Group is AssignedCouncil)
    public virtual ICollection<ProjectRequest> ProjectRequestsAssigned { get; set; } = new List<ProjectRequest>();
}
