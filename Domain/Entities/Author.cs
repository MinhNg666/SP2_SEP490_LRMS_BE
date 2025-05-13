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

    public int Role { get; set; }

    public int ProjectId { get; set; }

    public int? UserId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User? User { get; set; }

    // N-N relationship with Conference
    public virtual ICollection<AuthorConference> AuthorConferences { get; set; } = new List<AuthorConference>();

    // N-N relationship with Journal
    public virtual ICollection<AuthorJournal> AuthorJournals { get; set; } = new List<AuthorJournal>();
}
