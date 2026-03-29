using MediatR;
using Application.Services.Repositories;

namespace Application.Features.Payments.Commands.DeletePayment
{
    public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Unit>
    {
        private readonly IPaymentRepository _paymentRepo;

        public DeletePaymentCommandHandler(IPaymentRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken ct)
        {
            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId)
                ?? throw new KeyNotFoundException("Ödeme bulunamadı.");

            // Sadece ödemeyi ekleyen kişi silebilir
            if (payment.BorcluUserId != request.RequestingUserId)
                throw new UnauthorizedAccessException("Bu ödemeyi silme yetkiniz yok.");

            // Onaylanmış ödeme silinmez
            if (payment.Status == Domain.Enums.PaymentStatus.Approved)
                throw new InvalidOperationException("Onaylanmış ödeme silinemez.");

            // SOFT DELETE
            payment.IsDeleted = true;
            payment.DeletedAt = DateTime.UtcNow;
            payment.DeletedByUserId = request.RequestingUserId;

            await _paymentRepo.UpdateAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
