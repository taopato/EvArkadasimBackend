using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Houses.Dtos;
using Application.Services.PlannedExpenses;
using Application.Services.Repositories;
using MediatR;

namespace Application.Features.Houses.Queries.GetUserDebtBetween
{
    public class GetUserDebtBetweenQueryHandler : IRequestHandler<GetUserDebtBetweenQuery, PairDebtDetailDto>
    {
        private readonly ILedgerLineRepository _ledgerRepo;
        private readonly IPlannedExpenseLedgerSyncService _plannedExpenseLedgerSyncService;
        private readonly IUserRepository _userRepo;

        public GetUserDebtBetweenQueryHandler(
            ILedgerLineRepository ledgerRepo,
            IPlannedExpenseLedgerSyncService plannedExpenseLedgerSyncService,
            IUserRepository userRepo)
        {
            _ledgerRepo = ledgerRepo;
            _plannedExpenseLedgerSyncService = plannedExpenseLedgerSyncService;
            _userRepo = userRepo;
        }

        public async Task<PairDebtDetailDto> Handle(GetUserDebtBetweenQuery request, CancellationToken ct)
        {
            await _plannedExpenseLedgerSyncService.EnsureVisibleExpenseLedgersAsync(request.HouseId, ct);
            var nowUtc = System.DateTime.UtcNow;
            var lines = await _ledgerRepo.GetListAsync(
                l => l.HouseId == request.HouseId
                    && l.IsActive
                    && !l.IsClosed
                    && l.PostDate <= nowUtc
                    && ((l.FromUserId == request.UserAId && l.ToUserId == request.UserBId)
                     || (l.FromUserId == request.UserBId && l.ToUserId == request.UserAId)),
                ct);

            var aToB = lines
                .Where(l => l.FromUserId == request.UserAId && l.ToUserId == request.UserBId)
                .Sum(l => l.Amount - l.PaidAmount);

            var bToA = lines
                .Where(l => l.FromUserId == request.UserBId && l.ToUserId == request.UserAId)
                .Sum(l => l.Amount - l.PaidAmount);

            var userA = await _userRepo.GetByIdAsync(request.UserAId);
            var userB = await _userRepo.GetByIdAsync(request.UserBId);
            var userAName = $"{userA?.FirstName} {userA?.LastName}".Trim();
            var userBName = $"{userB?.FirstName} {userB?.LastName}".Trim();
            var netForUserA = bToA - aToB;

            if (aToB > bToA)
            {
                var amount = aToB - bToA;
                return new PairDebtDetailDto
                {
                    HouseId = request.HouseId,
                    UserAId = request.UserAId,
                    UserBId = request.UserBId,
                    UserAName = userAName,
                    UserBName = userBName,
                    BorcluUserId = request.UserAId,
                    AlacakliUserId = request.UserBId,
                    BorcluUserName = userAName,
                    AlacakliUserName = userBName,
                    Tutar = amount,
                    NetAmount = amount,
                    Net = netForUserA,
                    NetForUserId = request.UserAId
                };
            }

            if (bToA > aToB)
            {
                var amount = bToA - aToB;
                return new PairDebtDetailDto
                {
                    HouseId = request.HouseId,
                    UserAId = request.UserAId,
                    UserBId = request.UserBId,
                    UserAName = userAName,
                    UserBName = userBName,
                    BorcluUserId = request.UserBId,
                    AlacakliUserId = request.UserAId,
                    BorcluUserName = userBName,
                    AlacakliUserName = userAName,
                    Tutar = amount,
                    NetAmount = amount,
                    Net = netForUserA,
                    NetForUserId = request.UserAId
                };
            }

            return new PairDebtDetailDto
            {
                HouseId = request.HouseId,
                UserAId = request.UserAId,
                UserBId = request.UserBId,
                UserAName = userAName,
                UserBName = userBName,
                Tutar = 0m,
                NetAmount = 0m,
                Net = 0m,
                NetForUserId = request.UserAId
            };
        }
    }
}
