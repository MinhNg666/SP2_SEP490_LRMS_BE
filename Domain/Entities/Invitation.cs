using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Invitation
{
    public int InvitationId { get; set; }

    public int? Status { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? GroupId { get; set; }

    public DateTime? RespondDate { get; set; }

    public int? InvitedRole { get; set; }

    public int? RecieveBy { get; set; }

    public int? SentBy { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User? RecieveByNavigation { get; set; }

    public virtual User? SentByNavigation { get; set; }
}
