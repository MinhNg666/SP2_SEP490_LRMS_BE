using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("ResearchResource")]
public partial class ResearchResource
{
    [Key]
    [Column("resource_id")]
    public int ResourceId { get; set; }

    [Column("resource_name")]
    public string? ResourceName { get; set; }

    [Column("resource_quantity")]
    public int? ResourceQuantity { get; set; }

    [Column("resource_cost", TypeName = "decimal(18, 2)")]
    public decimal? ResourceCost { get; set; }

    [Column("resource_type")]
    public int? ResourceType { get; set; }

    [Column("resource_status")]
    public int? ResourceStatus { get; set; }

    // Foreign key to ProjectPhase
    [ForeignKey("ProjectPhase")]
    [Column("project_phase_id")]
    public int? ProjectPhaseId { get; set; }
    public virtual ProjectPhase? ProjectPhase { get; set; }

    // Foreign key to Project
    [ForeignKey("Project")]
    [Column("project_id")]
    public int? ProjectId { get; set; }
    public virtual Project? Project { get; set; }

    // Navigation to Documents (1-N)
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
