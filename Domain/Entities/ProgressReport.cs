using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("ProgressReport")]
public partial class ProgressReport
{
    [Key]
    public int ProgressReportId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? ReportDate { get; set; }

    public int? Status { get; set; } // e.g., Pending, Submitted, Approved, Rejected

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // N-1 Relationship with Project
    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    // N-1 Relationship with ProjectPhase (optional)
    [ForeignKey("ProjectPhase")]
    public int? ProjectPhaseId { get; set; }
    public virtual ProjectPhase? ProjectPhase { get; set; }

    // 1-N Relationship with CouncilVote
    public virtual ICollection<CouncilVote> CouncilVotes { get; set; } = new List<CouncilVote>();

    // 1-N Relationship with VoteResult
    public virtual ICollection<VoteResult> VoteResults { get; set; } = new List<VoteResult>();

    // 1-N Relationship with AssignReview
    public virtual ICollection<AssignReview> AssignReviews { get; set; } = new List<AssignReview>();

    // 1-N Relationship with Document
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    // 1-N Relationship with FundDisbursement
    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    // 1-N Relationship with ProjectRequest
    public virtual ICollection<ProjectRequest> ProjectRequests { get; set; } = new List<ProjectRequest>();
} 