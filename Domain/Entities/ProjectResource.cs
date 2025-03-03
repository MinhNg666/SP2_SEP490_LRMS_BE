using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class ProjectResource
{
    public int ResourceId { get; set; }

    public int? ProjectId { get; set; }

    public int? ResourceType { get; set; }

    public int? Quantity { get; set; }

    public bool? Acquired { get; set; }

    public decimal? EstimatedCost { get; set; }

    public virtual Project? Project { get; set; }
}
