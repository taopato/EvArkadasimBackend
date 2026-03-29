// Application/Features/Houses/Queries/GetUserDebts/GetUserDebtsQueryHandler.cs
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Features.Houses.Dtos;
using Application.Services.Repositories;
using MediatR;

namespace Application.Features.Houses.Queries.GetUserDebts
{
    public class GetUserDebtsQueryHandler : IRequestHandler<GetUserDebtsQuery, GetUserDebtsDto>
    {
        private readonly ILedgerLineRepository _ledgerRepo;
        public GetUserDebtsQueryHandler(ILedgerLineRepository ledgerRepo) => _ledgerRepo = ledgerRepo;

        public async Task<GetUserDebtsDto> Handle(GetUserDebtsQuery request, CancellationToken ct)
        {
            // Sadece açık ve aktif satırlar
            var lines = await _ledgerRepo.GetListAsync(
                l => l.HouseId == request.HouseId && l.IsActive && !l.IsClosed, ct);

            // Açık bakiye (Remaining) üstünden topla
            var pairForward = new Dictionary<(int from, int to), decimal>();
            var receivableByUser = new Dictionary<int, decimal>();
            var payableByUser = new Dictionary<int, decimal>();

            foreach (var l in lines)
            {
                var remaining = l.Amount - l.PaidAmount;
                if (remaining <= 0) continue;

                var key = (l.FromUserId, l.ToUserId);
                pairForward[key] = pairForward.TryGetValue(key, out var cur) ? cur + remaining : remaining;

                payableByUser[l.FromUserId] = payableByUser.TryGetValue(l.FromUserId, out var p) ? p + remaining : remaining;
                receivableByUser[l.ToUserId] = receivableByUser.TryGetValue(l.ToUserId, out var r) ? r + remaining : remaining;
            }

            var usersInHouse = new HashSet<int>(
                lines.Select(l => l.FromUserId).Concat(lines.Select(l => l.ToUserId)));

            // Net çiftler (A->B ile B->A netlenir)
            var netPairs = new List<GetUserDebtsDto.PairDebtDto>();
            foreach (var a in usersInHouse)
            {
                foreach (var b in usersInHouse)
                {
                    if (a >= b) continue;
                    var ab = pairForward.TryGetValue((a, b), out var abSum) ? abSum : 0m;
                    var ba = pairForward.TryGetValue((b, a), out var baSum) ? baSum : 0m;
                    var net = ab - ba;

                    if (net > 0) netPairs.Add(new() { FromUserId = a, ToUserId = b, NetAmount = net });
                    else if (net < 0) netPairs.Add(new() { FromUserId = b, ToUserId = a, NetAmount = -net });
                }
            }

            var totals = usersInHouse
                .Select(u => new GetUserDebtsDto.UserTotalDto
                {
                    UserId = u,
                    Receivable = receivableByUser.TryGetValue(u, out var r) ? r : 0m,
                    Payable = payableByUser.TryGetValue(u, out var p) ? p : 0m,
                    Net = (receivableByUser.TryGetValue(u, out r) ? r : 0m)
                        - (payableByUser.TryGetValue(u, out p) ? p : 0m)
                })
                .OrderByDescending(t => t.Net)
                .ToList();

            if (request.UserId.HasValue)
            {
                var uid = request.UserId.Value;
                netPairs = netPairs.Where(p => p.FromUserId == uid || p.ToUserId == uid).ToList();
                totals = totals.Where(t => t.UserId == uid).ToList();
            }

            return new GetUserDebtsDto { HouseId = request.HouseId, Pairs = netPairs, Totals = totals };
        }
    }
}
