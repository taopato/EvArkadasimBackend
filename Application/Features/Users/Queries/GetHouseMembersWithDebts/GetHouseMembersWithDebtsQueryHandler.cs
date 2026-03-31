using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Houses.Dtos;
using Application.Services.PlannedExpenses;
using Application.Services.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Houses.Queries.GetHouseMembersWithDebts
{
    public class GetHouseMembersWithDebtsQueryHandler
        : IRequestHandler<GetHouseMembersWithDebtsQuery, List<MemberDebtDto>>
    {
        private readonly IHouseRepository _houseRepo;
        private readonly ILedgerLineRepository _ledgerRepo;
        private readonly IMapper _mapper;
        private readonly IPlannedExpenseLedgerSyncService _plannedExpenseLedgerSyncService;

        public GetHouseMembersWithDebtsQueryHandler(
            IHouseRepository houseRepo,
            ILedgerLineRepository ledgerRepo,
            IPlannedExpenseLedgerSyncService plannedExpenseLedgerSyncService,
            IMapper mapper)
        {
            _houseRepo = houseRepo;
            _ledgerRepo = ledgerRepo;
            _plannedExpenseLedgerSyncService = plannedExpenseLedgerSyncService;
            _mapper = mapper;
        }

        public async Task<List<MemberDebtDto>> Handle(
            GetHouseMembersWithDebtsQuery request,
            CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            await _plannedExpenseLedgerSyncService.EnsureVisibleExpenseLedgersAsync(request.HouseId, cancellationToken);
            var house = await _houseRepo.GetByIdAsync(request.HouseId);
            var visibleLines = await _ledgerRepo.GetListAsync(
                l => l.HouseId == request.HouseId
                    && l.IsActive
                    && !l.IsClosed
                    && l.PostDate <= nowUtc,
                cancellationToken);

            var result = new List<MemberDebtDto>();
            foreach (var member in house.HouseMembers.Where(hm => hm.IsActive))
            {
                var uid = member.UserId;
                var alacak = visibleLines
                    .Where(l => l.ToUserId == uid)
                    .Sum(l => l.Amount - l.PaidAmount);
                var borc = visibleLines
                    .Where(l => l.FromUserId == uid)
                    .Sum(l => l.Amount - l.PaidAmount);

                result.Add(new MemberDebtDto
                {
                    UserId = uid,
                    FullName = $"{member.User.FirstName} {member.User.LastName}",
                    Email = member.User.Email,
                    Alacak = alacak,
                    Borc = borc
                });
            }

            return result;
        }
    }
}
