using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("ProposedResearchResource")]
public partial class ProposedResearchResource
{
    [Key]
    [Column("proposed_resource_id")]
    public int ProposedResourceId { get; set; }

    [Column("proposed_resource_name")]
    public string? ProposedResourceName { get; set; }

    [Column("proposed_resource_quantity")]
    public int? ProposedResourceQuantity { get; set; }

    [Column("proposed_resource_cost", TypeName = "decimal(18, 2)")]
    public decimal? ProposedResourceCost { get; set; }

    [Column("proposed_resource_type")]
    public int? ProposedResourceType { get; set; }

    [Column("proposed_resource_status")]
    public int? ProposedResourceStatus { get; set; }

    // Foreign key to ProjectRequest
    [ForeignKey("ProjectRequest")]
    [Column("project_request_id")]
    public int? ProjectRequestId { get; set; }
    public virtual ProjectRequest? ProjectRequest { get; set; }

    // Navigation to Documents (1-N)
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
