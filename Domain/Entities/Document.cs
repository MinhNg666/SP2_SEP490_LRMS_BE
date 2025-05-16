using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

public partial class Document
{
    public int DocumentId { get; set; }

    public DateTime? UploadAt { get; set; }

    public string? DocumentUrl { get; set; }

    public string? FileName { get; set; }

    public int? DocumentType { get; set; }

    public int? ProjectId { get; set; }

    public int? ProjectResourceId { get; set; }

    public int? ConferenceExpenseId { get; set; }

    public int? FundDisbursementId { get; set; }

    public int? UploadedBy { get; set; }

    public int? ProjectPhaseId { get; set; }

    public int? RequestId { get; set; } // Nullable RequestId

    [Column("conference_id")]
    public int? ConferenceId { get; set; }

    [Column("journal_id")]
    public int? JournalId { get; set; }

    [ForeignKey("JournalId")]
    public virtual Journal? Journal { get; set; }

    [ForeignKey("RequestId")]
    public virtual ProjectRequest? ProjectRequest { get; set; }

    public virtual ConferenceExpense? ConferenceExpense { get; set; }

    public virtual FundDisbursement? FundDisbursement { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ProjectResource? ProjectResource { get; set; }

    public virtual User? UploadedByNavigation { get; set; }

    public virtual ProjectPhase? ProjectPhase { get; set; }

    [ForeignKey("ConferenceId")]
    public virtual Conference? Conference { get; set; }

    // Foreign key to ProgressReport
    [ForeignKey("ProgressReport")]
    [Column("progress_report_id")]
    public int? ProgressReportId { get; set; }
    public virtual ProgressReport? ProgressReport { get; set; }

    // Foreign key to ResearchResource
    [ForeignKey("ResearchResource")]
    [Column("research_resource_id")]
    public int? ResearchResourceId { get; set; }
    public virtual ResearchResource? ResearchResource { get; set; }

    // Foreign key to ProposedResearchResource
    [ForeignKey("ProposedResearchResource")]
    [Column("proposed_research_resource_id")]
    public int? ProposedResearchResourceId { get; set; }
    public virtual ProposedResearchResource? ProposedResearchResource { get; set; }
}
