using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

public partial class Timeline
{
    [Key]
    public int TimelineId { get; set; }

    [ForeignKey("Sequence")]
    public int? SequenceId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Event { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? TimelineType { get; set; }

    public int? Status { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual TimelineSequence? Sequence { get; set; }
}
