using System.Linq;
using Domain.Entities;

namespace Application.Services.Repositories
{
    public interface IPaymentReadRepository
    {
        IQueryable<Payment> Query();
    }
}
