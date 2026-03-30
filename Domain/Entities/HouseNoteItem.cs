using System;

namespace Domain.Entities;

public class HouseNoteItem
{
    public int Id { get; set; }

    public int SectionId { get; set; }
    public HouseNoteSection Section { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CompletedByUserId { get; set; }
    public User? CompletedByUser { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
}
