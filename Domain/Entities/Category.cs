using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }
}
