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
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId) // Lọc theo userId
            .ToListAsync();
    }
    public async Task UpdateInvitation(Invitation invitation)
    {
        _context.Invitations.Update(invitation);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Notification>> GetByInvitationIdAsync(int invitationId)
    {
        return await _context.Notifications
            .Where(n => n.InvitationId == invitationId)
            .ToListAsync();
    }
}
