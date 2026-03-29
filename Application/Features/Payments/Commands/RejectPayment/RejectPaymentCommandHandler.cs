using Application.Services.Repositories;
using Domain.Enums;
using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Payments.Commands.RejectPayment
{
    public class RejectPaymentCommandHandler
        : IRequestHandler<RejectPaymentCommand, RejectPaymentResultDto>
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IHttpContextAccessor _http;

        public RejectPaymentCommandHandler(IPaymentRepository paymentRepo, IHttpContextAccessor http)
        {
            _paymentRepo = paymentRepo;
            _http = http;
        }

        public async Task<RejectPaymentResultDto> Handle(RejectPaymentCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId);
            if (payment is null) throw new InvalidOperationException("Ödeme bulunamadı.");

            // Yetki: sadece alacaklı kişi reddedebilir
            if (payment.AlacakliUserId != currentUserId)
                throw new UnauthorizedAccessException("Bu ödemeyi reddetme yetkiniz yok.");

            if (payment.Status == PaymentStatus.Rejected)
                throw new InvalidOperationException("Ödeme zaten reddedilmiş.");
            if (payment.Status == PaymentStatus.Approved)
                throw new InvalidOperationException("Onaylanmış ödeme reddedilemez.");

            payment.Status = PaymentStatus.Rejected;
            payment.RejectedDate = DateTime.UtcNow;
            payment.RejectedByUserId = currentUserId;

            await _paymentRepo.UpdateAsync(payment);

            return new RejectPaymentResultDto
            {
                PaymentId = payment.Id,
                RejectedDate = payment.RejectedDate.Value,
                RejectedByUserId = currentUserId
            };
        }

        private int GetCurrentUserId()
        {
            var principal = _http.HttpContext?.User;
            var id =
                principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal?.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(id))
                throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

            return int.Parse(id);
        }
    }
}
