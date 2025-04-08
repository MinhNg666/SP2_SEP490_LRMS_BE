using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Implementations;

namespace Repository.Interfaces;
public interface INotificationRepository : IGenericRepository<Notification>
{
    Task AddNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetNotificationsByUserId(int userId);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetByInvitationIdAsync(int invitationId);
}
