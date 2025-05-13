using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("AssignReview")] // Changed table name to singular for convention, or use "AssignReviews"
public partial class AssignReview // Changed class name to singular
{
    [Key]
    [Column("assign_id")]
    public int AssignId { get; set; }

    [Column("scheduled_date")]
    public DateTime? ScheduledDate { get; set; }

    [Column("scheduled_time")]
    public TimeSpan? ScheduledTime { get; set; }

    [Column("review_type")]
    public int? ReviewType { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    // Foreign key to User (Assign_By)
    [ForeignKey("AssignedByUser")]
    [Column("assigned_by_user_id")]
    public int? AssignedByUserId { get; set; }
    public virtual User? AssignedByUser { get; set; }

    // Foreign key to ProjectRequest
    [ForeignKey("ProjectRequest")]
    [Column("project_request_id")]
    public int? ProjectRequestId { get; set; }
    public virtual ProjectRequest? ProjectRequest { get; set; }

    // Foreign key to Inspection
    [ForeignKey("Inspection")]
    [Column("inspection_id")]
    public int? InspectionId { get; set; } // Requires Inspection entity
    public virtual Inspection? Inspection { get; set; } // Requires Inspection entity

    // Foreign key to FundDisbursement
    [ForeignKey("FundDisbursement")]
    [Column("fund_disbursement_id")]
    public int? FundDisbursementId { get; set; }
    public virtual FundDisbursement? FundDisbursement { get; set; }
} 