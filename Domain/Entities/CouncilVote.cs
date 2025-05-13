using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("CouncilVote")]
public partial class CouncilVote
{
    [Key]
    [Column("vote_id")]
    public int VoteId { get; set; }

    [Column("vote_status")]
    public int? VoteStatus { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    // Foreign key to ProjectRequest (N-1)
    [ForeignKey("ProjectRequest")]
    [Column("project_request_id")]
    public int ProjectRequestId { get; set; }
    public virtual ProjectRequest ProjectRequest { get; set; }

    // Foreign key to GroupMember (1-1), ensure GroupMemberId is unique if this is a strict 1-1 principal side
    [ForeignKey("GroupMember")]
    [Column("group_member_id")]
    public int GroupMemberId { get; set; } // This implies CouncilVote is dependent on GroupMember for the 1-1
    public virtual GroupMember GroupMember { get; set; }

    // Foreign key to Group (N-1)
    [ForeignKey("Group")]
    [Column("group_id")]
    public int GroupId { get; set; }
    public virtual Group Group { get; set; }

    // Foreign key to FundDisbursement (N-1 from CouncilVote's perspective)
    [ForeignKey("FundDisbursement")]
    [Column("fund_disbursement_id")]
    public int? FundDisbursementId { get; set; }
    public virtual FundDisbursement? FundDisbursement { get; set; }

    // Foreign key to Inspection (N-1 from CouncilVote's perspective)
    [ForeignKey("Inspection")]
    [Column("inspection_id")]
    public int? InspectionId { get; set; } // Requires Inspection entity
    public virtual Inspection? Inspection { get; set; } // Requires Inspection entity

    // Foreign key to VoteResult (N-1)
    [ForeignKey("VoteResult")]
    [Column("vote_result_id")]
    public int VoteResultId { get; set; }
    public virtual VoteResult VoteResult { get; set; }
} 