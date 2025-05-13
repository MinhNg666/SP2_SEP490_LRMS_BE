using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LRMS_API;

[Table("AuthorConference")]
public class AuthorConference
{
    [Key]
    public int AuthorConferenceId { get; set; } // Or a composite key

    [ForeignKey("Author")]
    public int AuthorId { get; set; }
    public virtual Author Author { get; set; }

    [ForeignKey("Conference")]
    public int ConferenceId { get; set; }
    public virtual Conference Conference { get; set; }
} 