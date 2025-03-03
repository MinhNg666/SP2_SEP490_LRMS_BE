using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Quota
{
    public int QuotaId { get; set; }

    public int? QuotaAmount { get; set; }

    public int? AllocatedBy { get; set; }

    public DateTime? AllocatedAt { get; set; }

    public int? ProjectId { get; set; }

    public decimal? LimitValue { get; set; }

    public decimal? CurrentValue { get; set; }

    public virtual User? AllocatedByNavigation { get; set; }

    public virtual Project? Project { get; set; }
}
