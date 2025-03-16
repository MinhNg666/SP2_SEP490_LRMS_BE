using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class ConferenceExpense
{
    public int ExpenseId { get; set; }

    public string? Accomodation { get; set; }

    public decimal? AccomodationExpense { get; set; }

    public string? Travel { get; set; }

    public decimal? TravelExpense { get; set; }

    public int? ConferenceId { get; set; }

    public virtual Conference? Conference { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
