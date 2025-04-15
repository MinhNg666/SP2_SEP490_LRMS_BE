using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class FundDisbursement
{
    public int FundDisbursementId { get; set; }

    public decimal? FundRequest { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Description { get; set; }

    public int SupervisorRequest { get; set; }

    public int AuthorRequest { get; set; }

    public int? AppovedBy { get; set; }

    public int? DisburseBy { get; set; }

    public int? ProjectId { get; set; }

    public int? QuotaId { get; set; }

    public int? ProjectPhaseId { get; set; }

    public virtual GroupMember? AppovedByNavigation { get; set; }

    public virtual Author AuthorRequestNavigation { get; set; } = null!;

    public virtual User? DisburseByNavigation { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Project? Project { get; set; }

    public virtual GroupMember SupervisorRequestNavigation { get; set; } = null!;

    public virtual Quota? Quota { get; set; }

    public virtual ProjectPhase? ProjectPhase { get; set; }
}
