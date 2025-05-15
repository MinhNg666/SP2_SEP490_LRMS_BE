using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("VoteResult")]
public partial class VoteResult
{
    [Key]
    [Column("result_id")]
    public int ResultId { get; set; }

    [Column("result_status")]
    public int? ResultStatus { get; set; }

    // Foreign key to Group (N-1 from VoteResult's perspective)
    [ForeignKey("Group")]
    [Column("group_id")]
    public int? GroupId { get; set; }
    public virtual Group? Group { get; set; }

    // Foreign key to ProjectRequest (N-1 from VoteResult's perspective)
    [ForeignKey("ProjectRequest")]
    [Column("project_request_id")]
    public int? ProjectRequestId { get; set; }
    public virtual ProjectRequest? ProjectRequest { get; set; }

    // Foreign key to Inspection (N-1 from VoteResult's perspective)
    [ForeignKey("Inspection")]
    [Column("inspection_id")]
    public int? InspectionId { get; set; } // Requires Inspection entity
    public virtual Inspection? Inspection { get; set; } // Requires Inspection entity

    // Foreign key to FundDisbursement (N-1 from VoteResult's perspective)
    [ForeignKey("FundDisbursement")]
    [Column("fund_disbursement_id")]
    public int? FundDisbursementId { get; set; }
    public virtual FundDisbursement? FundDisbursement { get; set; }

    // Navigation property to CouncilVotes (1-N)
    public virtual ICollection<CouncilVote> CouncilVotes { get; set; } = new List<CouncilVote>();

    // Foreign key to ProgressReport (N-1 from VoteResult's perspective)
    [ForeignKey("ProgressReport")]
    [Column("progress_report_id")]
    public int? ProgressReportId { get; set; }
    public virtual ProgressReport? ProgressReport { get; set; }
} 