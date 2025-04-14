using System;
using System.Collections.Generic;

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

    public virtual ConferenceExpense ConferenceExpense { get; set; } = null!;

    public virtual FundDisbursement? FundDisbursement { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ProjectResource ProjectResource { get; set; } = null!;

    public virtual User? UploadedByNavigation { get; set; }

    public virtual ProjectPhase? ProjectPhase { get; set; }
}
