using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense> AddAsync(Expense expense);
        Task<List<Expense>> GetAllAsync();
        Task<int> GetHouseMemberCountAsync(int kaydedenUserId);
        Task<List<HouseMember>> GetHouseMembersAsync(int houseId);
        Task<List<Expense>> GetByHouseIdAsync(int houseId);
        Task<List<Expense>> GetListAsync();
        Task<Expense?> GetByIdAsync(int id);
        Task DeleteAsync(Expense entity);
        Task<Expense> UpdateAsync(Expense entity); // ✅ Geri dönüş tipi Task<Expense>
        IQueryable<Expense> Query();
        Task SaveChangesAsync();


    }
}
