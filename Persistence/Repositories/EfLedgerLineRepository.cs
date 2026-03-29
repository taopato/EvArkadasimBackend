using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class EfLedgerLineRepository : ILedgerLineRepository
    {
        private readonly AppDbContext _ctx;
        public EfLedgerLineRepository(AppDbContext ctx) { _ctx = ctx; }

        public async Task AddAsync(LedgerLine line, CancellationToken ct = default)
        {
            await _ctx.LedgerLines.AddAsync(line, ct);
        }

        public async Task AddRangeAsync(IEnumerable<LedgerLine> lines, CancellationToken ct = default)
        {
            await _ctx.LedgerLines.AddRangeAsync(lines, ct);
        }

        public async Task UpdateAsync(LedgerLine line, CancellationToken ct = default)
        {
            _ctx.LedgerLines.Update(line);
            await _ctx.SaveChangesAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);

        public Task<LedgerLine?> GetByIdAsync(long id, CancellationToken ct = default)
            => _ctx.LedgerLines.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<LedgerLine>> GetListAsync(Expression<Func<LedgerLine, bool>> predicate, CancellationToken ct = default)
            => _ctx.LedgerLines.Where(predicate).AsNoTracking().ToListAsync(ct);

        public Task<List<LedgerLine>> GetOpenDebtsAsync(int houseId, int userId, CancellationToken ct = default)
            => _ctx.LedgerLines
                   .Where(x => x.HouseId == houseId && x.FromUserId == userId && x.IsActive && !x.IsClosed)
                   .AsNoTracking()
                   .ToListAsync(ct);

        public Task<List<LedgerLine>> GetOpenCreditsAsync(int houseId, int userId, CancellationToken ct = default)
            => _ctx.LedgerLines
                   .Where(x => x.HouseId == houseId && x.ToUserId == userId && x.IsActive && !x.IsClosed)
                   .AsNoTracking()
                   .ToListAsync(ct);

        public Task<List<LedgerLine>> ListOpenForPairAsync(
            int houseId, int fromUserId, int toUserId, DateTime asOf, CancellationToken ct = default)
            => _ctx.LedgerLines
                   .Where(x => x.HouseId == houseId
                            && x.FromUserId == fromUserId
                            && x.ToUserId == toUserId
                            && x.IsActive
                            && !x.IsClosed
                            && x.CreatedAt <= asOf)
                   .OrderBy(x => x.Id) // FIFO
                   .AsNoTracking()
                   .ToListAsync(ct);

        public async Task SoftDeleteByExpenseIdAsync(int expenseId, CancellationToken ct = default)
        {
            var lines = await _ctx.LedgerLines.Where(l => l.ExpenseId == expenseId && l.IsActive).ToListAsync(ct);
            if (lines.Count == 0) return;

            foreach (var l in lines)
            {
                l.IsActive = false;
                l.UpdatedAt = DateTime.UtcNow;
            }

            _ctx.LedgerLines.UpdateRange(lines);
            await _ctx.SaveChangesAsync(ct);
        }
    }
}
