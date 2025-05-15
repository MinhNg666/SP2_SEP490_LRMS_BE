using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

public partial class FundDisbursement
{
    public int FundDisbursementId { get; set; }

    public decimal? FundRequest { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Description { get; set; }

    public int? UserRequest { get; set; }

    public int? AppovedBy { get; set; }

    public int? DisburseBy { get; set; }

    public int? ProjectId { get; set; }

    public int? QuotaId { get; set; }

    public int? ProjectPhaseId { get; set; }

    public string? RejectionReason { get; set; }

    public int? ConferenceId { get; set; } 
    public int? JournalId { get; set; } 
    public int? FundDisbursementType { get; set; } 

    public int? ExpenseId { get; set; }
    public virtual ConferenceExpense? ConferenceExpense { get; set; }

    public virtual GroupMember? AppovedByNavigation { get; set; }

    public virtual User? UserRequestNavigation { get; set; }

    public virtual User? DisburseByNavigation { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Project? Project { get; set; }

    public virtual Quota? Quota { get; set; }

    public virtual ProjectPhase? ProjectPhase { get; set; }

    public virtual Conference? Conference { get; set; }
    public virtual Journal? Journal { get; set; }

    // Navigation to CouncilVotes (1-N)
    public virtual ICollection<CouncilVote> CouncilVotes { get; set; } = new List<CouncilVote>();

    // Navigation to VoteResults (1-N)
    public virtual ICollection<VoteResult> VoteResults { get; set; } = new List<VoteResult>();

    // Navigation to AssignReviews (1-N)
    public virtual ICollection<AssignReview> AssignReviews { get; set; } = new List<AssignReview>();

    // Foreign key to ProgressReport
    [ForeignKey("ProgressReport")]
    [Column("progress_report_id")]
    public int? ProgressReportId { get; set; }
    public virtual ProgressReport? ProgressReport { get; set; }
}
