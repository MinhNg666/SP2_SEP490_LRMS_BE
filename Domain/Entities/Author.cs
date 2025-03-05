using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Author
{
    public int AuthorId { get; set; }

    public int? ProjectId { get; set; }

    public int? UserId { get; set; }

    public int? Role { get; set; }

    public string? Email { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
