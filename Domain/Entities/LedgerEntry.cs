using System;
using Domain.Enums;

namespace Domain.Entities
{
    // Kim kime ne borç (Bill finalize edildiğinde oluşur)
    public class LedgerEntry
    {
        public int Id { get; set; }
        public int HouseId { get; set; }

        public int FromUserId { get; set; } // borçlu
        public int ToUserId { get; set; }   // alacaklı (genelde ResponsibleUser)

        public decimal Amount { get; set; } // toplam borç
        public decimal PaidAmount { get; set; } // ödenen toplam (allocations toplamı)
        public bool IsClosed { get; set; }  // Amount == PaidAmount

        public int? BillId { get; set; }
        public UtilityType? UtilityType { get; set; }
        public string? Month { get; set; } // "YYYY-MM"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}
