using Application.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Entities;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HouseNotesController : ControllerBase
{
    private readonly IHouseNoteRepository _houseNoteRepository;
    private readonly IHouseRepository _houseRepository;

    public HouseNotesController(IHouseNoteRepository houseNoteRepository, IHouseRepository houseRepository)
    {
        _houseNoteRepository = houseNoteRepository;
        _houseRepository = houseRepository;
    }

    [HttpGet("{houseId:int}")]
    public async Task<IActionResult> GetBoard(int houseId)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(houseId, userId))
        {
            return Forbid();
        }

        var sections = await _houseNoteRepository.GetSectionsByHouseIdAsync(houseId);
        var response = new
        {
            houseId,
            sections = sections.Select(section => new
            {
                id = section.Id,
                title = section.Title,
                createdAt = section.CreatedAt,
                deletedAt = section.DeletedAt,
                items = section.Items
                    .Where(item => item.DeletedAt == null && !item.IsCompleted)
                    .OrderBy(item => item.CreatedAt)
                    .Select(MapItem),
                completedItems = section.Items
                    .Where(item => item.DeletedAt == null && item.IsCompleted)
                    .OrderByDescending(item => item.CompletedAt ?? item.CreatedAt)
                    .Select(MapItem)
            })
        };

        return Ok(response);
    }

    [HttpPost("{houseId:int}/sections")]
    public async Task<IActionResult> CreateSection(int houseId, [FromBody] CreateHouseNoteSectionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(houseId, userId))
        {
            return Forbid();
        }

        var title = (request.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new { message = "Baslik bos olamaz." });
        }

        var section = await _houseNoteRepository.AddSectionAsync(new HouseNoteSection
        {
            HouseId = houseId,
            Title = title,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        return Ok(new
        {
            id = section.Id,
            title = section.Title,
            createdAt = section.CreatedAt,
            items = Array.Empty<object>(),
            completedItems = Array.Empty<object>()
        });
    }

    [HttpPost("sections/{sectionId:int}/items")]
    public async Task<IActionResult> CreateItem(int sectionId, [FromBody] CreateHouseNoteItemRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        var section = await _houseNoteRepository.GetSectionByIdAsync(sectionId);
        if (section == null)
        {
            return NotFound(new { message = "Not basligi bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(section.HouseId, userId))
        {
            return Forbid();
        }

        var content = (request.Content ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new { message = "Liste maddesi bos olamaz." });
        }

        var item = await _houseNoteRepository.AddItemAsync(new HouseNoteItem
        {
            SectionId = sectionId,
            Content = content,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        return Ok(MapItem(item));
    }

    [HttpPost("items/{itemId:int}/complete")]
    public async Task<IActionResult> CompleteItem(int itemId)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        var item = await _houseNoteRepository.GetItemByIdAsync(itemId);
        if (item == null || item.DeletedAt != null)
        {
            return NotFound(new { message = "Liste maddesi bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(item.Section.HouseId, userId))
        {
            return Forbid();
        }

        item.IsCompleted = true;
        item.CompletedAt = DateTime.UtcNow;
        item.CompletedByUserId = userId;
        await _houseNoteRepository.SaveChangesAsync();

        return Ok(MapItem(item));
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        var item = await _houseNoteRepository.GetItemByIdAsync(itemId);
        if (item == null || item.DeletedAt != null)
        {
            return NotFound(new { message = "Liste maddesi bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(item.Section.HouseId, userId))
        {
            return Forbid();
        }

        item.DeletedAt = DateTime.UtcNow;
        item.DeletedByUserId = userId;
        await _houseNoteRepository.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpDelete("sections/{sectionId:int}")]
    public async Task<IActionResult> DeleteSection(int sectionId)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Gecerli kullanici bulunamadi." });
        }

        var section = await _houseNoteRepository.GetSectionByIdAsync(sectionId);
        if (section == null || section.DeletedAt != null)
        {
            return NotFound(new { message = "Not basligi bulunamadi." });
        }

        if (!await _houseRepository.IsActiveMemberAsync(section.HouseId, userId))
        {
            return Forbid();
        }

        var deletedAt = DateTime.UtcNow;
        section.DeletedAt = deletedAt;
        section.DeletedByUserId = userId;
        await _houseNoteRepository.SoftDeleteActiveItemsBySectionIdAsync(sectionId, userId, deletedAt);
        await _houseNoteRepository.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }

    private static object MapItem(HouseNoteItem item)
    {
        return new
        {
            id = item.Id,
            content = item.Content,
            createdAt = item.CreatedAt,
            createdByUserId = item.CreatedByUserId,
            isCompleted = item.IsCompleted,
            completedAt = item.CompletedAt,
            completedByUserId = item.CompletedByUserId
        };
    }

    public sealed class CreateHouseNoteSectionRequest
    {
        public string? Title { get; set; }
    }

    public sealed class CreateHouseNoteItemRequest
    {
        public string? Content { get; set; }
    }
}
