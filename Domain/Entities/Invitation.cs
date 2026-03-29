// Yol: Domain/Entities/Invitation.cs
using System;

namespace Domain.Entities
{
    public class Invitation
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public int HouseId { get; set; }
        public DateTime SentAt { get; set; }

        public House House { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ExpiresAt { get; set; }
        public int? AcceptedByUserId { get; set; }
        public DateTime AcceptedAt { get; set; }
        

    }
}
