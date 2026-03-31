using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PlannedExpenses
{
    public interface IPlannedExpenseLedgerSyncService
    {
        Task EnsureVisibleExpenseLedgersAsync(int houseId, CancellationToken ct = default);
    }
}
