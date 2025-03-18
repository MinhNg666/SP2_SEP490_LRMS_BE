using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;

public class InvitationRepository : GenericRepository<InvitationRepository>, IInvitationRepository
{
    private readonly LRMSDbContext _context;

    public InvitationRepository(LRMSDbContext context)
    {
        _context = context;
    }

    public async Task AddInvitationAsync(Invitation invitation)
    {
        await _context.Invitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Invitation>> GetInvitationsByUserId(int userId)
    {
        return await Task.FromResult(_context.Invitations.Where(i => i.InvitationId == userId));
    }
    public async Task<Invitation> GetInvitationById(int invitationId) // Thêm phương thức này
    {
        return await _context.Invitations.FindAsync(invitationId);
    }
    public async Task UpdateInvitation(Invitation invitation)
    {
        _context.Invitations.Update(invitation);
        await _context.SaveChangesAsync();
    }
}
