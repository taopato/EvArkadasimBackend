using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class LedgerLine
    {
        public long Id { get; set; }                  // ← int → long

        public int HouseId { get; set; }
        public int ExpenseId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PostDate { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0m;

        public bool IsClosed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
