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
                             .Include(h => h.HouseMembers.Where(m => m.IsActive))
                             .ThenInclude(m => m.User)
                             .AsNoTracking()
                             .ToListAsync();

        public async Task<House> GetByIdAsync(int id)
        {
            return await _context.Houses
                .Include(h => h.HouseMembers.Where(m => m.IsActive))
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(h => h.Id == id)
                ?? throw new KeyNotFoundException("House bulunamadı.");
        }

        public async Task AddMemberAsync(HouseMember member)
        {
            var existing = await _context.HouseMembers
                .FirstOrDefaultAsync(m => m.HouseId == member.HouseId && m.UserId == member.UserId);

            if (existing == null)
            {
                await _context.HouseMembers.AddAsync(member);
            }
            else
            {
                existing.IsActive = true;
                existing.LeftAt = null;
                existing.RemovedByUserId = null;
                existing.JoinedDate = member.JoinedDate == default ? DateTime.UtcNow : member.JoinedDate;
                _context.HouseMembers.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsActiveMemberAsync(int houseId, int userId)
        {
            return await _context.HouseMembers
                .AnyAsync(m => m.HouseId == houseId && m.UserId == userId && m.IsActive);
        }

        public async Task RemoveMemberAsync(int houseId, int userId)
        {
            var entry = await _context.HouseMembers
                .FirstOrDefaultAsync(m => m.HouseId == houseId && m.UserId == userId);
            if (entry != null)
            {
                // Soft delete mantığına çeviriyoruz
                entry.IsActive = false;
                entry.LeftAt = DateTime.UtcNow;
                _context.HouseMembers.Update(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(House entity)
        {
            _context.Houses.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int houseId)
        {
            var house = await _context.Houses.FindAsync(houseId);
            if (house != null)
            {
                _context.Houses.Remove(house);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException("Ev grubu silinemedi. Evde bagli harcama veya odeme kayitlari olabilir.");
                }
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
