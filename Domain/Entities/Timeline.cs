using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Timeline
{
    public int TimelineId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TimelineType { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
