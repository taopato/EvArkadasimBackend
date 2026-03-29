using System;
using System.Collections.Generic;

namespace Application.Features.Payments.Commands.AddPaymentWithAllocations
{
    public class AddPaymentWithAllocationsDto
    {
        public int HouseId { get; set; }
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public decimal Tutar { get; set; }
        public string? DekontUrl { get; set; }
        public DateTime? OdemeTarihi { get; set; }
        public string? Aciklama { get; set; }
        public int? ChargeId { get; set; }
        public List<AllocationItem> Allocations { get; set; } = new();
    }

    public class AllocationItem
    {
        public long LedgerLineId { get; set; }        // ← int → long
        public decimal Amount { get; set; }
    }
}
