using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository.Implementations;
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly LRMSDbContext _context;
    public NotificationRepository(LRMSDbContext context) : base(context)
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
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
    }
}
