using System;
using System.Collections.Generic;

namespace LRMS_API;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public int? ProjectId { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? InvitationId { get; set; }

    public virtual Invitation? Invitation { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
