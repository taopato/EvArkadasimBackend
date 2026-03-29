using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class EfHouseRepository : IHouseRepository
    {
        private readonly AppDbContext _context;

        public EfHouseRepository(AppDbContext context)
            => _context = context;

        public async Task<House> AddAsync(House entity)
        {
            var added = await _context.Houses.AddAsync(entity);
            await _context.SaveChangesAsync();
            return added.Entity;
        }

        public async Task<List<House>> GetAllAsync()
            => await _context.Houses
                             .Include(h => h.HouseMembers)
                             .ThenInclude(m => m.User)
                             .AsNoTracking()
                             .ToListAsync();

        public async Task<House> GetByIdAsync(int id)
        {
            return await _context.Houses
                .Include(h => h.HouseMembers)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(h => h.Id == id)
                ?? throw new KeyNotFoundException("House bulunamadı.");
        }

        public async Task AddMemberAsync(HouseMember member)
        {
            await _context.HouseMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int houseId, int userId)
        {
            var entry = await _context.HouseMembers
                .FirstOrDefaultAsync(m => m.HouseId == houseId && m.UserId == userId);
            if (entry != null)
            {
                _context.HouseMembers.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }
    }
}
