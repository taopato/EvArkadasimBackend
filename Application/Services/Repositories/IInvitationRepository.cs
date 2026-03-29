using Domain.Entities;

namespace Application.Services.Repositories;

public interface IInvitationRepository
{
    Task<Invitation> AddAsync(Invitation invitation);
    Task<List<Invitation>> GetByUserIdAsync(int userId);
    Task<Invitation?> GetByCodeAsync(string code);
    Task UpdateAsync(Invitation invitation);

}
