using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class EfHouseNoteRepository : IHouseNoteRepository
{
    private readonly AppDbContext _context;

    public EfHouseNoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HouseNoteSection>> GetSectionsByHouseIdAsync(int houseId)
    {
        return await _context.Set<HouseNoteSection>()
            .Include(section => section.Items)
            .Where(section => section.HouseId == houseId && section.DeletedAt == null)
            .OrderBy(section => section.Title)
            .ThenBy(section => section.Id)
            .ToListAsync();
    }

    public async Task<HouseNoteSection?> GetSectionByIdAsync(int sectionId)
    {
        return await _context.Set<HouseNoteSection>()
            .Include(section => section.Items)
            .FirstOrDefaultAsync(section => section.Id == sectionId);
    }

    public async Task<HouseNoteItem?> GetItemByIdAsync(int itemId)
    {
        return await _context.Set<HouseNoteItem>()
            .Include(item => item.Section)
            .FirstOrDefaultAsync(item => item.Id == itemId);
    }

    public async Task<HouseNoteSection> AddSectionAsync(HouseNoteSection section)
    {
        var entry = await _context.Set<HouseNoteSection>().AddAsync(section);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<HouseNoteItem> AddItemAsync(HouseNoteItem item)
    {
        var entry = await _context.Set<HouseNoteItem>().AddAsync(item);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<int> SoftDeleteActiveItemsBySectionIdAsync(int sectionId, int userId, DateTime deletedAt)
    {
        var items = await _context.Set<HouseNoteItem>()
            .Where(item => item.SectionId == sectionId && item.DeletedAt == null && !item.IsCompleted)
            .ToListAsync();

        foreach (var item in items)
        {
            item.DeletedAt = deletedAt;
            item.DeletedByUserId = userId;
        }

        return items.Count;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
