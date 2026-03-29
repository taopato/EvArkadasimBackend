using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Services.Repositories
{
    public interface IPaymentAllocationRepository
    {
        Task AddRangeAsync(IEnumerable<PaymentAllocation> allocations, CancellationToken ct = default);
        Task<decimal> GetPaidAmountForLedgerLineAsync(long ledgerLineId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
