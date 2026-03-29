using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class EfHouseMemberRepository : IHouseMemberRepository
    {
        private readonly AppDbContext _context;

        public EfHouseMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<HouseMember> Query()
        {
            return _context.HouseMembers.Where(hm => hm.IsActive).AsQueryable();
        }

        public async Task<List<HouseMember>> GetByHouseIdAsync(int houseId)
        {
            return await _context.HouseMembers
                .Where(h => h.HouseId == houseId && h.IsActive)
                .Include(h => h.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<House> GetByIdWithMembersAsync(int houseId)
        {
            return await _context.Houses
                .Include(h => h.HouseMembers.Where(hm => hm.IsActive))
                    .ThenInclude(hm => hm.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == houseId);
        }

        public async Task<List<int>> GetActiveUserIdsAsync(int houseId, CancellationToken ct = default)
        {
            return await _context.HouseMembers
                .AsNoTracking()
                .Where(hm => hm.HouseId == houseId && hm.IsActive)
                .Select(hm => hm.UserId)
                .ToListAsync(ct);
        }

        public async Task<HouseMember?> GetByHouseAndUserAsync(int houseId, int userId)
        {
            return await _context.HouseMembers
                .FirstOrDefaultAsync(hm => hm.HouseId == houseId && hm.UserId == userId && hm.IsActive);
        }

        public async Task UpdateAsync(HouseMember member)
        {
            _context.HouseMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
