using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

public partial class ProjectResource
{
    public int ProjectResourceId { get; set; }

    public string ResourceName { get; set; } = null!;

    public int? ResourceType { get; set; }

    public decimal? Cost { get; set; }

    public int? Quantity { get; set; }

    public bool? Acquired { get; set; }

    public int? ProjectId { get; set; }

    [ForeignKey("ProjectPhase")]
    [Column("project_phase_id")]
    public int? ProjectPhaseId { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Project? Project { get; set; }

    public virtual ProjectPhase? ProjectPhase { get; set; }
}
