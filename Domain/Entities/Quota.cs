using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Quota
{
    public int QuotaId { get; set; }

    public decimal? AllocatedBudget { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? ProjectId { get; set; }

    public int? AllocatedBy { get; set; }

    public virtual User? AllocatedByNavigation { get; set; }

    public virtual Project? Project { get; set; }

     public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();
}
