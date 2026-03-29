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
    public class EfPaymentAllocationRepository : IPaymentAllocationRepository
    {
        private readonly AppDbContext _ctx;
        public EfPaymentAllocationRepository(AppDbContext ctx) { _ctx = ctx; }

        public async Task AddRangeAsync(IEnumerable<PaymentAllocation> allocations, CancellationToken ct = default)
        {
            await _ctx.PaymentAllocations.AddRangeAsync(allocations, ct);
        }

        public Task<decimal> GetPaidAmountForLedgerLineAsync(long ledgerLineId, CancellationToken ct = default)
            => _ctx.PaymentAllocations
                   .Where(x => x.LedgerLineId == ledgerLineId)
                   .Select(x => x.Amount)
                   .DefaultIfEmpty(0m)
                   .SumAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);
    }
}
