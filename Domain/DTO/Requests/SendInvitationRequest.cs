using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;

public class SendInvitationRequest
{
    public int InvitationId { get; set; }

    public int? Status { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int GroupId { get; set; }
    public int? ProjectId { get; set; }

    public int InvitedUserId { get; set; }

    public int InvitedBy { get; set; }

    public int InvitedRole { get; set; }

    public DateTime? RespondDate { get; set; }
}
