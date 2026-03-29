using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class EfInvitationRepository : IInvitationRepository
{
    private readonly AppDbContext _context;

    public EfInvitationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invitation> AddAsync(Invitation invitation)
    {
        var added = await _context.Invitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
        return added.Entity;
    }

    public async Task<List<Invitation>> GetByUserIdAsync(int userId)
    {
        return await _context.Invitations
            .Where(i => i.AcceptedByUserId == userId)
            .ToListAsync();
    }

    public async Task<Invitation?> GetByCodeAsync(string code)
    {
        return await _context.Invitations
            .FirstOrDefaultAsync(i => i.Token == code);
    }

    public async Task UpdateAsync(Invitation invitation)
    {
        _context.Invitations.Update(invitation);
        await _context.SaveChangesAsync();
    }

}
