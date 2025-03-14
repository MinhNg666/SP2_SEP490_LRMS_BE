using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("Author")]
public partial class Author
{
    [Column("author_id")]
    public int AuthorId { get; set; }

    [Column("project_id")]
    public int? ProjectId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("role")]
    public int? Role { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
