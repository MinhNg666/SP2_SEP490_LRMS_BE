using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Conference
{
    public int ConferenceId { get; set; }

    public string? ConferenceName { get; set; }

    public int? ConferenceRanking { get; set; }

    public string? Location { get; set; }

    public DateTime? PresentationDate { get; set; }

    public DateTime? AcceptanceDate { get; set; }

    public int? PresentationType { get; set; }

    public int? ProjectId { get; set; }

    public virtual ICollection<ConferenceExpense> ConferenceExpenses { get; set; } = new List<ConferenceExpense>();

    public virtual Project? Project { get; set; }
}
