using Domain.Entities;
using System.Linq.Expressions;

public interface IRecurringChargeRepository
{
    IQueryable<RecurringCharge> Query();
    Task<RecurringCharge?> GetAsync(Expression<Func<RecurringCharge, bool>> predicate, CancellationToken ct = default);
    Task<RecurringCharge> AddAsync(RecurringCharge entity, CancellationToken ct = default);
    Task UpdateAsync(RecurringCharge entity, CancellationToken ct = default);
}
