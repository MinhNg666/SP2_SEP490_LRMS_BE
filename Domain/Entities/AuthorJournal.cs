using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("AuthorJournal")]
public class AuthorJournal
{
    [Key]
    public int AuthorJournalId { get; set; } // Or a composite key

    [ForeignKey("Author")]
    public int AuthorId { get; set; }
    public virtual Author Author { get; set; }

    [ForeignKey("Journal")]
    public int JournalId { get; set; }
    public virtual Journal Journal { get; set; }
} 