using Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime RegistrationDate { get; set; }
    public ICollection<HouseMember> HouseMembers { get; set; } = new List<HouseMember>();

    [MaxLength(16)]
    public string? PhoneNumber { get; set; }

    [MaxLength(26)]
    public string? Iban { get; set; }

    [MaxLength(1024)]
    public string? ProfileImageUrl { get; set; }

    // --- SOFT DELETE ---
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
}
