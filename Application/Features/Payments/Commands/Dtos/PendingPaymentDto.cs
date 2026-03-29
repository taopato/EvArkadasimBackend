using System;

namespace Application.Features.Payments.Queries.GetPendingPayments
{
    public class PendingPaymentDto
    {
        public int Id { get; set; }
        public int HouseId { get; set; }

        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }

        public string BorcluUserName { get; set; } = string.Empty;

        public decimal Tutar { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "Cash" | "BankTransfer"
        public DateTime? OdemeTarihi { get; set; }
        public string? Aciklama { get; set; }
        public string? DekontUrl { get; set; }
        public string Status { get; set; } = "Pending";

        // 🔽 UI’nin beklediği ek alanlar:
        public int? ChargeId { get; set; }      // Hangi cycle için katkı
        public string? Type { get; set; }       // Rent | Electric | Internet | ...
        public string? Period { get; set; }     // "YYYY-MM"
    }
}
