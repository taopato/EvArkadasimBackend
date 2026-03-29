using System;
using Domain.Enums; // <-- eklendi

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

        // wwwroot/uploads/payments/ altında sakladığın dosyanın göreli (public) yolu
        public string DekontUrl { get; set; } = string.Empty;

        // Ödemenin yapıldığı tarih (gelmezse API'de UtcNow veriyoruz)
        public DateTime OdemeTarihi { get; set; }

        // Opsiyonel açıklama
        public string? Aciklama { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool AlacakliOnayi { get; set; } = false; // <- non-nullable ve default

        // --- YENİ ALANLAR ---
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer; // Nakit/IBAN
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? RejectedDate { get; set; }
        public int? RejectedByUserId { get; set; }
        public int? ChargeId { get; set; }           // NEW
        public ChargeCycle? Charge { get; set; }     // NEW (isteğe bağlı navigation)
    }
}
