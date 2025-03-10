using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly LRMSDbContext _context;
    public NotificationRepository(LRMSDbContext context)
    {
        _context = context;
    }
    public async Task AddNotificationAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Notification>> GetNotificationsByUserId(int userId)
    {
        return await Task.FromResult(_context.Notifications.Where(n => n.UserId == userId));
    }
}
