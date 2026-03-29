using Application.Services.Repositories;
using Domain.Entities;                    // ChargeCycleStatus, RecurringCharge, ChargeCycle
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandHandler
        : IRequestHandler<CreatePaymentCommand, CreatedPaymentResponseDto>
    {
        private readonly IPaymentRepository _paymentRepo;

        // ⬇️ EKLENDİ: cycle/contract doğrulaması için
        private readonly IChargeCycleRepository _cycleRepo;
        private readonly IRecurringChargeRepository _recurringRepo;

        public CreatePaymentCommandHandler(
            IPaymentRepository paymentRepo,
            IChargeCycleRepository cycleRepo,
            IRecurringChargeRepository recurringRepo)
        {
            _paymentRepo = paymentRepo;
            _cycleRepo = cycleRepo;        // ⬅️ set
            _recurringRepo = recurringRepo;    // ⬅️ set
        }

        public async Task<CreatedPaymentResponseDto> Handle(
            CreatePaymentCommand request,
            CancellationToken cancellationToken)
        {
            // ----------------------------------------------------------------
            // 1) (Opsiyonel) ChargeId bağlamı geldiyse doğrula
            // ----------------------------------------------------------------
            if (request.ChargeId.HasValue)
            {
                // cycle var mı?
                var cycle = await _cycleRepo.GetAsync(
                    c => c.Id == request.ChargeId.Value, cancellationToken);

                if (cycle is null)
                    throw new InvalidOperationException("Charge not found");

                // contract var mı?
                var contract = await _recurringRepo.GetAsync(
                    r => r.Id == cycle.ContractId, cancellationToken);

                if (contract is null)
                    throw new InvalidOperationException("Contract not found");

                // alacaklı, sözleşmenin payer'ı mı?
                if (contract.PayerUserId != request.AlacakliUserId)
                    throw new InvalidOperationException("Alacaklı, sözleşmenin payer'ı olmalı");

                // cycle zaten Paid mi?
                if (cycle.Status == ChargeCycleStatus.Paid)
                    throw new InvalidOperationException("Cycle already paid");
            }

            // ----------------------------------------------------------------
            // 2) Payment entity oluştur
            // (Mevcut alanları aynen korudum; sadece ChargeId ekliyorum.)
            // ----------------------------------------------------------------
            var payment = new Payment
            {
                HouseId = request.HouseId,
                BorcluUserId = request.BorcluUserId,
                AlacakliUserId = request.AlacakliUserId,
                Tutar = request.Tutar,
                DekontUrl = request.DekontUrl,
                OdemeTarihi = request.OdemeTarihi == default ? DateTime.UtcNow : request.OdemeTarihi,
                Aciklama = request.Aciklama ?? string.Empty, // null gelirse boş
                CreatedDate = DateTime.UtcNow,

                // ⬇️ NEW: varsa cycle bağlamını yaz
                ChargeId = request.ChargeId
            };

            // (Projende PaymentStatus/PaymentMethod varsa burada set edebilirsin)
            // payment.Status = PaymentStatus.Pending;
            // payment.PaymentMethod = request.PaymentMethod;

            // ----------------------------------------------------------------
            // 3) Kaydet + response
            // ----------------------------------------------------------------
            var created = await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            return new CreatedPaymentResponseDto
            {
                Id = created.Id,
                HouseId = created.HouseId,
                BorcluUserId = created.BorcluUserId,
                AlacakliUserId = created.AlacakliUserId,
                Tutar = created.Tutar,
                DekontUrl = created.DekontUrl,
                OdemeTarihi = created.OdemeTarihi,
                Aciklama = created.Aciklama
            };
        }
    }
}
