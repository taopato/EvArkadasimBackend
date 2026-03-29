using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.Commands.AddPaymentWithAllocations
{
    public class AddPaymentWithAllocationsCommandHandler
        : IRequestHandler<AddPaymentWithAllocationsCommand, AddPaymentWithAllocationsResult>
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ILedgerLineRepository _ledgerRepo;
        private readonly IPaymentAllocationRepository _allocRepo;

        public AddPaymentWithAllocationsCommandHandler(
            IPaymentRepository paymentRepo,
            ILedgerLineRepository ledgerRepo,
            IPaymentAllocationRepository allocRepo)
        {
            _paymentRepo = paymentRepo;
            _ledgerRepo = ledgerRepo;
            _allocRepo = allocRepo;
        }

        public async Task<AddPaymentWithAllocationsResult> Handle(AddPaymentWithAllocationsCommand request, CancellationToken ct)
        {
            var m = request.Model;
            var odemeTarihi = m.OdemeTarihi ?? DateTime.UtcNow;

            var payment = new Payment
            {
                HouseId = m.HouseId,
                BorcluUserId = m.BorcluUserId,
                AlacakliUserId = m.AlacakliUserId,
                Tutar = m.Tutar,
                PaymentMethod = PaymentMethod.BankTransfer,
                DekontUrl = m.DekontUrl,
                OdemeTarihi = odemeTarihi,
                Aciklama = m.Aciklama,
                ChargeId = m.ChargeId,
                Status = PaymentStatus.Approved,
                ApprovedDate = DateTime.UtcNow,
                ApprovedByUserId = m.AlacakliUserId
            };

            await _paymentRepo.AddAsync(payment);      // ct’siz
            var allocations = new List<PaymentAllocation>();

            foreach (var a in m.Allocations ?? Enumerable.Empty<AllocationItem>())
            {
                allocations.Add(new PaymentAllocation
                {
                    PaymentId = payment.Id,
                    LedgerLineId = a.LedgerLineId, // long
                    Amount = a.Amount,
                    CreatedAt = DateTime.UtcNow   // ⬅️ EKLE

                });

                var line = await _ledgerRepo.GetByIdAsync(a.LedgerLineId); // ct’siz
                if (line != null && line.IsActive)
                {
                    line.PaidAmount += a.Amount;
                    line.IsClosed = line.PaidAmount >= line.Amount;
                    line.UpdatedAt = DateTime.UtcNow;

                    await _ledgerRepo.UpdateAsync(line); // ct’siz
                }
            }

            if (allocations.Count > 0)
                await _allocRepo.AddRangeAsync(allocations);

            // tek seferde commit
            await _paymentRepo.SaveChangesAsync();     // ct’siz
            await _allocRepo.SaveChangesAsync();       // ct’siz

            return new AddPaymentWithAllocationsResult { PaymentId = payment.Id };
        }
    }
}
