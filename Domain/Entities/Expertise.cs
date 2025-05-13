using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Added for ICollection

namespace LRMS_API;

[Table("Expertise")]
public partial class Expertise
{
    [Key]
    [Column("expertise_id")]
    public int ExpertiseId { get; set; }

    [Column("expertise_name")]
    public string? ExpertiseName { get; set; }

    [Column("expertise_status")]
    public int? ExpertiseStatus { get; set; }

    // Foreign key to User (N-1 from Expertise's perspective)
    [ForeignKey("User")]
    [Column("user_id")]
    public int? UserId { get; set; } // Nullable if an expertise can exist without a user, or not assigned initially
    public virtual User? User { get; set; }
} 