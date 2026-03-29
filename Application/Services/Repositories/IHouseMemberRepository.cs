using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Repositories
{
    public interface IHouseMemberRepository
    {
        Task<List<HouseMember>> GetByHouseIdAsync(int houseId);
        IQueryable<HouseMember> Query();
        Task<House> GetByIdWithMembersAsync(int houseId);
        Task<List<int>> GetActiveUserIdsAsync(int houseId, CancellationToken ct = default);
        Task<HouseMember?> GetByHouseAndUserAsync(int houseId, int userId);
        Task UpdateAsync(HouseMember member);
        Task SaveChangesAsync();
    }
}
