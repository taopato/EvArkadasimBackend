using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Features.LedgerLines.Dtos;
using Application.Services.Repositories;
using MediatR;

namespace Application.Features.LedgerLines.Queries.GetLedgerLinesByHouse
{
    public class GetLedgerLinesByHouseQueryHandler
        : IRequestHandler<GetLedgerLinesByHouseQuery, List<LedgerLineDto>>
    {
        private readonly ILedgerLineRepository _repo;

        public GetLedgerLinesByHouseQueryHandler(ILedgerLineRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<LedgerLineDto>> Handle(GetLedgerLinesByHouseQuery request, CancellationToken ct)
        {
            var lines = await _repo.GetListAsync(l => l.HouseId == request.HouseId && l.IsActive);

            // Sistem alanlarını sızdırmadan map’liyoruz
            return lines
                .OrderBy(l => l.PostDate)
                .Select(l => new LedgerLineDto
                {
                    Id = l.Id,
                    HouseId = l.HouseId,
                    ExpenseId = l.ExpenseId,
                    FromUserId = l.FromUserId,
                    ToUserId = l.ToUserId,
                    Amount = l.Amount,
                    PostDate = l.PostDate
                })
                .ToList();
        }
    }
}
