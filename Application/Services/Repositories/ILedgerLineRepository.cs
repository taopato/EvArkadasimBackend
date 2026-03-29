using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Services.Repositories
{
    public interface ILedgerLineRepository
    {
        Task AddAsync(LedgerLine line, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<LedgerLine> lines, CancellationToken ct = default);
        Task UpdateAsync(LedgerLine line, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task<LedgerLine?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<List<LedgerLine>> GetListAsync(Expression<Func<LedgerLine, bool>> predicate, CancellationToken ct = default);

        Task<List<LedgerLine>> GetOpenDebtsAsync(int houseId, int userId, CancellationToken ct = default);
        Task<List<LedgerLine>> GetOpenCreditsAsync(int houseId, int userId, CancellationToken ct = default);

        /// <summary>FIFO için açık satırları döner (CreatedAt &lt;= asOf filtresi ile).</summary>
        Task<List<LedgerLine>> ListOpenForPairAsync(
            int houseId, int fromUserId, int toUserId, DateTime asOf, CancellationToken ct = default);

        /// <summary>Belirli harcamaya ait satırları pasifleştirir (soft delete).</summary>
        Task SoftDeleteByExpenseIdAsync(int expenseId, CancellationToken ct = default);
    }
}
