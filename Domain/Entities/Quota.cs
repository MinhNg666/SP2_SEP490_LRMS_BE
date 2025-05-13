using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

public partial class Quota
{
    public int QuotaId { get; set; }

    public decimal? AllocatedBudget { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? ProjectId { get; set; }

    public int? AllocatedBy { get; set; }

    [Column("num_projects")]
    public int? NumProjects { get; set; }

    [Column("quota_year")]
    public int? QuotaYear { get; set; }

    [ForeignKey("Department")]
    public int? DepartmentId { get; set; }

    public virtual User? AllocatedByNavigation { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ICollection<FundDisbursement> FundDisbursements { get; set; } = new List<FundDisbursement>();

    public virtual Department? Department { get; set; }
}
