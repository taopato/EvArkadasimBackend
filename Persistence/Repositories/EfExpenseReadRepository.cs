using System.Linq;
using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    // <--- BURADA SINIF ADI EfExpenseReadRepository olacak
    public class EfExpenseReadRepository : IExpenseReadRepository
    {
        private readonly AppDbContext _ctx;
        public EfExpenseReadRepository(AppDbContext ctx) => _ctx = ctx;

        public IQueryable<Expense> Query() => _ctx.Expenses.AsQueryable();
    }
}
