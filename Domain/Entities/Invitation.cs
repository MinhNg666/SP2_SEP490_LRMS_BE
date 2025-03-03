using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Invitation
{
    public int InvitationId { get; set; }

    public int? Status { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? GroupId { get; set; }

    public int? InvitedUserId { get; set; }

    public int? InvitedBy { get; set; }

    public int? InvitedRole { get; set; }

    public DateTime? RespondDate { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? InvitedByNavigation { get; set; }

    public virtual User? InvitedUser { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
