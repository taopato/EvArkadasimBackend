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
    public class EfRecurringChargeRepository : IRecurringChargeRepository
    {
        private readonly AppDbContext _context;
        public EfRecurringChargeRepository(AppDbContext context) => _context = context;

        public IQueryable<RecurringCharge> Query() => _context.Set<RecurringCharge>().AsQueryable();

        public async Task<RecurringCharge?> GetAsync(Expression<Func<RecurringCharge, bool>> predicate, CancellationToken ct = default)
            => await _context.Set<RecurringCharge>().FirstOrDefaultAsync(predicate, ct);

        public async Task<RecurringCharge> AddAsync(RecurringCharge entity, CancellationToken ct = default)
        {
            _context.Set<RecurringCharge>().Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(RecurringCharge entity, CancellationToken ct = default)
        {
            _context.Set<RecurringCharge>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
