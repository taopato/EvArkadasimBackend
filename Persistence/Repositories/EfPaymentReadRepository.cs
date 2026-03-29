using System.Linq;
using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class EfPaymentReadRepository : IPaymentReadRepository
    {
        private readonly AppDbContext _ctx;
        public EfPaymentReadRepository(AppDbContext ctx) => _ctx = ctx;

        public IQueryable<Payment> Query() => _ctx.Payments.AsQueryable();
    }
}
