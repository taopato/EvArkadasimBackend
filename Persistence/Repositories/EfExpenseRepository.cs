using Application.Services.Repositories;
using Domain.Entities;
using Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class EfExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public EfExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            var added = await _context.Set<Expense>().AddAsync(expense);
            //await _context.SaveChangesAsync(); // ❌ Artık SaveChanges burada değil
            return added.Entity;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<int> GetHouseMemberCountAsync(int kaydedenUserId)
        {
            return await _context.HouseMembers
                                 .CountAsync(hm => hm.UserId == kaydedenUserId);
        }

        public async Task<List<HouseMember>> GetHouseMembersAsync(int houseId)
        {
            return await _context.HouseMembers
                                 .Where(m => m.HouseId == houseId)
                                 .ToListAsync();
        }

        public async Task<List<Expense>> GetByHouseIdAsync(int houseId)
        {
            return await _context.Expenses
                                 .Where(e => e.HouseId == houseId)
                                 .ToListAsync();
        }

        public async Task<List<Expense>> GetListAsync()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses.FindAsync(id);
        }

        public async Task DeleteAsync(Expense entity)
        {
            _context.Expenses.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Expense> UpdateAsync(Expense entity)
        {
            _context.Expenses.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public IQueryable<Expense> Query()
        {
            return _context.Expenses.AsQueryable();
        }
    }
}
