using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Implementations;

namespace Repository.Interfaces;

public interface IInvitationRepository : IGenericRepository<Invitation>
{
    Task AddInvitationAsync(Invitation invitation);
    Task<IEnumerable<Invitation>> GetInvitationsByUserId(int userId);
    Task<Invitation> GetInvitationById(int invitationId);
    Task UpdateInvitation(Invitation invitation);
    Task<IEnumerable<Invitation>> GetPendingInvitationsByGroupId(int groupId);

}
