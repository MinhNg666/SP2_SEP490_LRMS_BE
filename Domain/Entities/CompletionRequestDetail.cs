using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API; // Or your appropriate namespace for Entities

public partial class CompletionRequestDetail
{
    [Key]
    [Column("completion_detail_id")]
    public int CompletionDetailId { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("budget_remaining", TypeName = "decimal(18, 2)")]
    public decimal? BudgetRemaining { get; set; } // Nullable

    [Column("budget_reconciled")]
    public bool BudgetReconciled { get; set; }

    [Column("completion_summary")]
    public string? CompletionSummary { get; set; } // Nullable

    [Column("budget_variance_explanation")]
    public string? BudgetVarianceExplanation { get; set; } // Nullable

    // Navigation Property (Back to the ProjectRequest)
    [ForeignKey("RequestId")]
    public virtual ProjectRequest? ProjectRequest { get; set; }
}
