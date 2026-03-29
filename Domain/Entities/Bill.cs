using System;
using System.Collections.Generic;
using System.IO;
using Domain.Enums;

namespace Domain.Entities
{
    public class Bill
    {
        public int Id { get; set; }
        public int HouseId { get; set; }
        public UtilityType UtilityType { get; set; }

        // "YYYY-MM" formatı (örn. "2025-09")
        public string Month { get; set; } = null!;

        public int ResponsibleUserId { get; set; } // snapshot
        public BillStatus Status { get; set; } = BillStatus.Draft;

        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Note { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinalizedAt { get; set; }

        public ICollection<BillShare> Shares { get; set; } = new List<BillShare>();
        public ICollection<BillDocument> Documents { get; set; } = new List<BillDocument>();
    }
}
