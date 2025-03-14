using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;
public interface IInvitationService
{
    Task SendInvitation(SendInvitationRequest request);
    Task<IEnumerable<InvitationResponse>> GetInvitationsByUserId(int userId);
    Task AcceptInvitation(int invitationId, int userId);
    Task RejectInvitation(int invitationId, int userId);
}
