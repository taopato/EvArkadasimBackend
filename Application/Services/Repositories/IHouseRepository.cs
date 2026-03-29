using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Repositories
{
    public interface IHouseRepository
    {
        Task<House> AddAsync(House entity);
        Task<List<House>> GetAllAsync();
        Task<House> GetByIdAsync(int id);
        Task AddMemberAsync(HouseMember member);
        Task RemoveMemberAsync(int houseId, int userId);

    }
}
