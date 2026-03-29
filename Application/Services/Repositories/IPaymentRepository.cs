// Application/Services/Repositories/IPaymentRepository.cs
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Services.Repositories
{
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetAllAsync();
        // Yeni metodlar:
        Task<decimal> GetTotalAlacaklıAsync(int houseId, int userId);
        Task<decimal> GetTotalBorçluAsync(int houseId, int userId);
        Task<Payment> AddAsync(Payment entity);
        Task<List<Payment>> GetByHouseIdAsync(int houseId);
        IQueryable<Payment> Query();
        Task SaveChangesAsync();
        Task<Payment?> GetByIdAsync(int id); // tek ödeme
        Task<IList<Payment>> GetPendingByAlacakliAsync(int alacakliUserId); // bekleyenler
        Task UpdateAsync(Payment entity);

    }
}
