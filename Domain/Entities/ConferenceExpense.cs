using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class ConferenceExpense
{
    public int ExpenseId { get; set; }

    public int? ConferenceId { get; set; }

    public decimal? TravelExpense { get; set; }

    public decimal? TransportationExpense { get; set; }

    public string? Transportation { get; set; }

    public string? Accomodation { get; set; }

    public virtual Conference? Conference { get; set; }
}
