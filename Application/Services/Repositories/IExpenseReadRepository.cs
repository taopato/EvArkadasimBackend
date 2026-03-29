using System.Linq;
using Domain.Entities;

namespace Application.Services.Repositories
{
    public interface IExpenseReadRepository
    {
        IQueryable<Expense> Query();

    }
}
