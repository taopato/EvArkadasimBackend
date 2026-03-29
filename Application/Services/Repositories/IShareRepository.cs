// Application/Services/Repositories/IShareRepository.cs
using Domain.Entities;

public interface IShareRepository
{
    Task AddRangeAsync(List<Share> shares);
    Task SaveChangesAsync();
    Task<Share> AddAsync(Share entity);
    Task DeleteAsync(Share entity);


}
