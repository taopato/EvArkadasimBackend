// Application/Features/Payments/Commands/ApprovePayment/ApprovePaymentCommandHandler.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.Repositories;
using Domain.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Core.Exceptions;

namespace Application.Features.Payments.Commands.ApprovePayment
{
    public class ApprovePaymentCommandHandler
        : IRequestHandler<ApprovePaymentCommand, ApprovePaymentResultDto>
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ILedgerLineRepository _ledgerRepo;
        private readonly IPaymentAllocationRepository _allocationRepo;
        private readonly IHttpContextAccessor _http;

        public ApprovePaymentCommandHandler(
            IPaymentRepository paymentRepo,
            ILedgerLineRepository ledgerRepo,
            IPaymentAllocationRepository allocationRepo,
            IHttpContextAccessor http)
        {
            _paymentRepo = paymentRepo;
            _ledgerRepo = ledgerRepo;
            _allocationRepo = allocationRepo;
            _http = http;
        }

        public async Task<ApprovePaymentResultDto> Handle(ApprovePaymentCommand request, CancellationToken ct)
        {
            var currentUserId = GetCurrentUserId();
            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId)
                          ?? throw new InvalidOperationException("Ödeme bulunamadı.");

            // YETKİ
            if (payment.AlacakliUserId != currentUserId)
                throw new ForbiddenAccessException("Bu ödemeyi onaylama yetkiniz yok.");

            if (payment.Status == PaymentStatus.Approved)
                throw new InvalidOperationException("Ödeme zaten onaylanmış.");
            if (payment.Status == PaymentStatus.Rejected)
                throw new InvalidOperationException("Reddedilmiş ödeme onaylanamaz.");
            if (payment.Tutar <= 0)
                throw new InvalidOperationException("Geçersiz ödeme tutarı.");

            // 1) FIFO açık ledger satırlarını çek (LINE)
            var openLines = await _ledgerRepo.ListOpenForPairAsync(
            payment.HouseId,
            payment.BorcluUserId,
            payment.AlacakliUserId,
            DateTime.UtcNow,
    ct
            );
            // 2) Tutarı FIFO paylaştır
            decimal remaining = payment.Tutar;
            var allocations = new List<PaymentAllocation>();

            foreach (var line in openLines)
            {
                if (remaining <= 0) break;

                var lineRemaining = line.Amount - line.PaidAmount;
                if (lineRemaining <= 0) continue;

                var apply = Math.Min(lineRemaining, remaining);

                // allocation kaydı (LINE hedef)
                allocations.Add(new PaymentAllocation
                {
                    PaymentId = payment.Id,
                    LedgerLineId = line.Id,
                    Amount = apply,
                    CreatedAt = DateTime.UtcNow    // ⬅️ EKLE

                });

                // ledger güncelle (LINE)
                line.PaidAmount += apply;
                line.IsClosed = line.PaidAmount >= line.Amount;
                line.UpdatedAt = DateTime.UtcNow;

                remaining -= apply;
            }

            // Mevcut borcu aşan tutar, gönderen kişi lehine alacak oluşturur.
            // Böylece kullanıcı borcu olmasa bile avans/borç verme amaçlı ödeme yapabilir.
            if (remaining > 0)
            {
                await _ledgerRepo.AddAsync(new LedgerLine
                {
                    HouseId = payment.HouseId,
                    ExpenseId = null,
                    SourcePaymentId = payment.Id,
                    FromUserId = payment.AlacakliUserId,
                    ToUserId = payment.BorcluUserId,
                    Amount = remaining,
                    PaidAmount = 0m,
                    PostDate = payment.OdemeTarihi,
                    IsActive = true,
                    IsClosed = false,
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            await _ledgerRepo.UpdateRangeAsync(openLines, ct);

            // 3) Allocation'ları topluca ekle
            if (allocations.Count > 0)
                await _allocationRepo.AddRangeAsync(allocations, ct);

            // 4) Ödemeyi onayla
            payment.Status = PaymentStatus.Approved;
            payment.ApprovedDate = DateTime.UtcNow;
            payment.ApprovedByUserId = currentUserId;
            await _paymentRepo.UpdateAsync(payment);

            // Ledger, allocation ve payment değişiklikleri tek commit'te yazılır.
            await _paymentRepo.SaveChangesAsync();

            return new ApprovePaymentResultDto
            {
                PaymentId = payment.Id,
                ApprovedDate = payment.ApprovedDate!.Value,
                ApprovedByUserId = currentUserId
            };
        }

        private int GetCurrentUserId()
        {
            var principal = _http.HttpContext?.User;
            var id = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? principal?.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(id))
                throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");
            return int.Parse(id);
        }
    }
}
