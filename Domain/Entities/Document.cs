using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Document
{
    public int DocumentId { get; set; }

    public int? ProjectId { get; set; }

    public int? MilestoneId { get; set; }

    public string? DocumentUrl { get; set; }

    public string? FileName { get; set; }

    public int? DocumentType { get; set; }

    public DateTime? UploadAt { get; set; }

    public int? UploadBy { get; set; }

    public virtual Milestone? Milestone { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? UploadByNavigation { get; set; }
}
