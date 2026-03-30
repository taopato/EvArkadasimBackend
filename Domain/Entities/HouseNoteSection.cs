using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class HouseNoteSection
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public House House { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

    public ICollection<HouseNoteItem> Items { get; set; } = new List<HouseNoteItem>();
}
