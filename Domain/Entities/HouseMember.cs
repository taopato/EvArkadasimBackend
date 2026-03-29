using System;

namespace Domain.Entities
{
    public class HouseMember
    {
        public int HouseId { get; set; }
        public House House { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }
}
