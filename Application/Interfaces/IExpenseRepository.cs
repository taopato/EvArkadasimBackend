// Application/Interfaces/IExpenseRepository.cs
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IExpenseRepository
    {
        // Core.Persistence.Repositories.IEntityRepository<T> kullanmıyorsanız
        Task<List<Expense>> GetListAsync();
    }
}
