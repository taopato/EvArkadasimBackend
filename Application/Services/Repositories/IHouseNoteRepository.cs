using Domain.Entities;

namespace Application.Services.Repositories;

public interface IHouseNoteRepository
{
    Task<List<HouseNoteSection>> GetSectionsByHouseIdAsync(int houseId);
    Task<HouseNoteSection?> GetSectionByIdAsync(int sectionId);
    Task<HouseNoteItem?> GetItemByIdAsync(int itemId);
    Task<HouseNoteSection> AddSectionAsync(HouseNoteSection section);
    Task<HouseNoteItem> AddItemAsync(HouseNoteItem item);
    Task<int> SoftDeleteActiveItemsBySectionIdAsync(int sectionId, int userId, DateTime deletedAt);
    Task SaveChangesAsync();
}
