using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("Inspection")]
public partial class Inspection
{
    [Key]
    [Column("inspection_id")]
    public int InspectionId { get; set; }

    [Column("result")]
    public string? Result { get; set; }

    [Column("result_rating")]
    public int? ResultRating { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("inspection_status")]
    public int? InspectionStatus { get; set; }

    // Foreign key to Project (N-1)
    [ForeignKey("Project")]
    [Column("project_id")]
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }

    // Navigation properties to entities that reference Inspection
    public virtual ICollection<CouncilVote> CouncilVotes { get; set; } = new List<CouncilVote>();
    public virtual ICollection<VoteResult> VoteResults { get; set; } = new List<VoteResult>();
    public virtual ICollection<AssignReview> AssignReviews { get; set; } = new List<AssignReview>();

    // Navigation to ProjectRequests (1-N)
    public virtual ICollection<ProjectRequest> ProjectRequests { get; set; } = new List<ProjectRequest>();
} 