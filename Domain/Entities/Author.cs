using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Author
{
    public int AuthorId { get; set; }

    public int Role { get; set; }

    public int ProjectId { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    public virtual Project Project { get; set; } = null!;

    public virtual User? User { get; set; }
}
