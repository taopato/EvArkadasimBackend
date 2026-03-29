using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class House
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Bir ev grubunun üyeleri
        public ICollection<HouseMember> HouseMembers { get; set; } = new List<HouseMember>();

        // Evi oluşturan kullanıcı
        public int CreatorUserId { get; set; }
        public virtual User? CreatorUser { get; set; }
    }
}
