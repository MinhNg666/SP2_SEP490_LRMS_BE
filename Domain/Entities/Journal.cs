using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Journal
{
    public int JournalId { get; set; }

    public int? ProjectId { get; set; }

    public bool? PublisherApproval { get; set; }

    public string? DoiNumber { get; set; }

    public string? Pages { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? AcceptanceDate { get; set; }

    public DateTime? PublicationDate { get; set; }

    public string? JournalName { get; set; }

    public string? ReviewerComments { get; set; }

    public string? RevisionHistory { get; set; }

    public virtual Project? Project { get; set; }
}
