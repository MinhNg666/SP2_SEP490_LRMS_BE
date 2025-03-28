using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;
public class CreateNotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public int? ProjectId { get; set; }
    public int? Status { get; set; }
    public bool? IsRead { get; set; }
    public int? InvitationId { get; set; }
}
