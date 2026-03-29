// Persistence/Repositories/EfPaymentRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class EfPaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public EfPaymentRepository(AppDbContext context) => _context = context;

        // --- MEVCUT METODLARIN ---
        public async Task<List<Payment>> GetAllAsync()
            => await _context.Payments.ToListAsync();

        public async Task<decimal> GetTotalAlacaklıAsync(int houseId, int userId)
            // houseId’i şimdilik kullanmıyoruz
            => await _context.Payments
                .Where(p => p.AlacakliUserId == userId)
                .SumAsync(p => (decimal?)p.Tutar) ?? 0m;

        public async Task<decimal> GetTotalBorçluAsync(int houseId, int userId)
            => await _context.Payments
                .Where(p => p.BorcluUserId == userId)
                .SumAsync(p => (decimal?)p.Tutar) ?? 0m;

        public async Task<Payment> AddAsync(Payment entity)
        {
            var added = await _context.Payments.AddAsync(entity);
            await _context.SaveChangesAsync();
            return added.Entity;
        }

        public async Task<List<Payment>> GetByHouseIdAsync(int houseId)
        {
            return await _context.Payments
                                 .Include(p => p.BorcluUser)
                                 .Include(p => p.AlacakliUser)
                                 .Where(p => p.HouseId == houseId)
                                 .OrderByDescending(p => p.OdemeTarihi)
                                 .ToListAsync();
        }

        public IQueryable<Payment> Query()
        {
            return _context.Payments.AsQueryable();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- YENİ EKLENENLER ---
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.BorcluUser)
                .Include(p => p.AlacakliUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IList<Payment>> GetPendingByAlacakliAsync(int alacakliUserId)
        {
            return await _context.Payments
                .Include(p => p.BorcluUser)
                .Where(p => p.AlacakliUserId == alacakliUserId
                         && p.Status == Domain.Enums.PaymentStatus.Pending)
                .OrderByDescending(p => p.OdemeTarihi)
                .ToListAsync();
        }

        public async Task UpdateAsync(Payment entity)
        {
            _context.Payments.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
