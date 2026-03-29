using System;

namespace Application.Features.Payments.Commands.CreatePayment
{
    public class CreatedPaymentResponseDto
    {
        public int Id { get; set; }
        public int HouseId { get; set; }
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public decimal Tutar { get; set; }
        public string DekontUrl { get; set; } = string.Empty;
        public DateTime OdemeTarihi { get; set; }
        public string? Aciklama { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
