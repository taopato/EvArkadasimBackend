using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Queries.GetPendingPayments
{
    public class GetPendingPaymentsQueryHandler
        : IRequestHandler<GetPendingPaymentsQuery, IList<PendingPaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IChargeCycleRepository _cycleRepo;
        private readonly IRecurringChargeRepository _recRepo;

        public GetPendingPaymentsQueryHandler(
            IPaymentRepository paymentRepo,
            IChargeCycleRepository cycleRepo,
            IRecurringChargeRepository recRepo)
        {
            _paymentRepo = paymentRepo;
            _cycleRepo = cycleRepo;
            _recRepo = recRepo;
        }

        public async Task<IList<PendingPaymentDto>> Handle(GetPendingPaymentsQuery request, CancellationToken cancellationToken)
        {
            // 1) Payer (alacaklı) için Pending ödemeleri çek
            var list = await _paymentRepo.GetPendingByAlacakliAsync(request.UserId);

            // 2) Enrich için chargeId’leri topla
            var chargeIds = list.Where(p => p.ChargeId.HasValue)
                                .Select(p => p.ChargeId!.Value)
                                .Distinct()
                                .ToList();

            var cycles = chargeIds.Count == 0
                ? new List<Domain.Entities.ChargeCycle>()
                : await _cycleRepo.Query()
                    .Where(c => chargeIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

            var contractIds = cycles.Select(c => c.ContractId).Distinct().ToList();

            var contracts = contractIds.Count == 0
                ? new List<Domain.Entities.RecurringCharge>()
                : await _recRepo.Query()
                    .Where(r => contractIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

            // 3) DTO map + enrich (Type, Period)
            var result = list.Select(p =>
            {
                var dto = new PendingPaymentDto
                {
                    Id = p.Id,
                    HouseId = p.HouseId,
                    BorcluUserId = p.BorcluUserId,
                    AlacakliUserId = p.AlacakliUserId,
                    BorcluUserName = p.BorcluUser != null
                        ? $"{p.BorcluUser.FirstName} {p.BorcluUser.LastName}".Trim()
                        : string.Empty,
                    Tutar = p.Tutar,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    OdemeTarihi = p.OdemeTarihi,
                    Aciklama = p.Aciklama,
                    DekontUrl = p.DekontUrl,
                    Status = p.Status.ToString(),
                    ChargeId = p.ChargeId
                };

                if (p.ChargeId.HasValue)
                {
                    var ch = cycles.FirstOrDefault(x => x.Id == p.ChargeId.Value);
                    if (ch != null)
                    {
                        dto.Period = ch.Period;
                        var rc = contracts.FirstOrDefault(x => x.Id == ch.ContractId);
                        if (rc != null)
                            dto.Type = rc.Type.ToString();
                    }
                }

                return dto;
            }).ToList();

            return result;
        }
    }
}
