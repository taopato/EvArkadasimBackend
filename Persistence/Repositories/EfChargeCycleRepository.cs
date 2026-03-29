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
    public class EfChargeCycleRepository : IChargeCycleRepository
    {
        private readonly AppDbContext _context;
        public EfChargeCycleRepository(AppDbContext context) => _context = context;

        public IQueryable<ChargeCycle> Query() => _context.Set<ChargeCycle>().AsQueryable();

        public async Task<ChargeCycle?> GetAsync(Expression<Func<ChargeCycle, bool>> predicate, CancellationToken ct = default)
            => await _context.Set<ChargeCycle>().FirstOrDefaultAsync(predicate, ct);

        public async Task<ChargeCycle> AddAsync(ChargeCycle entity, CancellationToken ct = default)
        {
            _context.Set<ChargeCycle>().Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(ChargeCycle entity, CancellationToken ct = default)
        {
            _context.Set<ChargeCycle>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
