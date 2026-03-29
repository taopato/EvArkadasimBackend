using Domain.Entities;
using System.Linq.Expressions;

public interface IChargeCycleRepository
{
    IQueryable<ChargeCycle> Query();
    Task<ChargeCycle?> GetAsync(Expression<Func<ChargeCycle, bool>> predicate, CancellationToken ct = default);
    Task<ChargeCycle> AddAsync(ChargeCycle entity, CancellationToken ct = default);
    Task UpdateAsync(ChargeCycle entity, CancellationToken ct = default);
}
