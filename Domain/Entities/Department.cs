using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public int? ProjectId { get; set; }

    public int? UserId { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
