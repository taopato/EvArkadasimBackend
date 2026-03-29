using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int HouseId { get; set; }
        public House House { get; set; } = null!;

        public int BorcluUserId { get; set; }
        public User BorcluUser { get; set; } = null!;

        public int AlacakliUserId { get; set; }
        public User AlacakliUser { get; set; } = null!;

        public decimal Tutar { get; set; }

        public string DekontUrl { get; set; } = string.Empty;

        public DateTime OdemeTarihi { get; set; }

        public string? Aciklama { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool AlacakliOnayi { get; set; } = false;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? RejectedDate { get; set; }
        public int? RejectedByUserId { get; set; }
        public int? ChargeId { get; set; }
        public ChargeCycle? Charge { get; set; }

        // --- SOFT DELETE ---
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
    }
}
