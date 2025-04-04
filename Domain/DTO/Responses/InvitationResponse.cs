﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses;
public class InvitationResponse
{
    public int InvitationId { get; set; }
    public int? Status { get; set; }
    public string? Message { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? GroupId { get; set; }
    public int? InvitedBy { get; set; }
    public int? InvitedRole { get; set; }
    public DateTime? RespondDate { get; set; }
    public int? SentBy { get; set; }
}
