using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class EfPersonalExpenseRepository : IPersonalExpenseRepository
    {
        private readonly AppDbContext _context;
        public EfPersonalExpenseRepository(AppDbContext context)
            => _context = context;

        public async Task<PersonalExpense> AddAsync(PersonalExpense entity)
        {
            var added = await _context.PersonalExpenses.AddAsync(entity);
            await _context.SaveChangesAsync();
            return added.Entity;
        }
        public async Task DeleteAsync(PersonalExpense entity)
        {
            _context.PersonalExpenses.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
