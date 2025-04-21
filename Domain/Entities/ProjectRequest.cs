using Domain.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API; // Or your appropriate namespace for Entities

public partial class ProjectRequest
{
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("phase_id")]
    public int? PhaseId { get; set; } // Nullable

    [Column("timeline_id")]
    public int? TimelineId { get; set; } // Nullable

    [Column("request_type")]
    public ProjectRequestTypeEnum RequestType { get; set; } // Changed int to ProjectRequestTypeEnum

    [Column("requested_by")]
    public int RequestedById { get; set; }

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; }

    [Column("assigned_council")]
    public int? AssignedCouncilId { get; set; } // Nullable

    [Column("approval_status")]
    public ApprovalStatusEnum? ApprovalStatus { get; set; } // Changed int? to ApprovalStatusEnum?

    [Column("approved_by")]
    public int? ApprovedById { get; set; } // Nullable

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; } // Nullable

    [Column("rejection_reason")]
    public string? RejectionReason { get; set; } // Nullable

    // Navigation Properties
    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }

    [ForeignKey("PhaseId")]
    public virtual ProjectPhase? ProjectPhase { get; set; }

    [ForeignKey("TimelineId")]
    public virtual Timeline? Timeline { get; set; }

    [ForeignKey("RequestedById")]
    public virtual User? RequestedBy { get; set; }

    [ForeignKey("AssignedCouncilId")]
    public virtual Group? AssignedCouncil { get; set; }

    [ForeignKey("ApprovedById")]
    public virtual User? ApprovedBy { get; set; }

    // Relationship to CompletionRequestDetails (One-to-one)
    public virtual CompletionRequestDetail? CompletionRequestDetail { get; set; }
}
