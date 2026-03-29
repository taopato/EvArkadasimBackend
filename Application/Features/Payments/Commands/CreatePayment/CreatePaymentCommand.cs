using Application.Features.Payments.Dtos;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Payments.Commands.CreatePayment
{
    // Application'da ASP türü YOK; dosya yolu (DekontUrl) string olarak gelir
    public class CreatePaymentCommand : IRequest<CreatedPaymentResponseDto>
    {
        public int HouseId { get; set; }
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public decimal Tutar { get; set; }
        public string DekontUrl { get; set; } = string.Empty;
        public DateTime OdemeTarihi { get; set; }
        public string? Aciklama { get; set; }
        public PaymentMethod PaymentMethod { get; set; } // Enum olacak
        public int? ChargeId { get; set; }

    }
}
