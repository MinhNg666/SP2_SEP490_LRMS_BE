using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LRMS_API;

public partial class TimelineSequence
{
    [Key]
    public int SequenceId { get; set; }

    public string? SequenceName { get; set; }

    public string? SequenceDescription { get; set; }

    public string? SequenceColor { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Timeline> Timelines { get; set; } = new List<Timeline>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
} 