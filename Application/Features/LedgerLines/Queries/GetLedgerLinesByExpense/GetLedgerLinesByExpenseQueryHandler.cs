using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Features.LedgerLines.Dtos;
using Application.Services.Repositories;
using MediatR;

namespace Application.Features.LedgerLines.Queries.GetLedgerLinesByExpense
{
    public class GetLedgerLinesByExpenseQueryHandler
        : IRequestHandler<GetLedgerLinesByExpenseQuery, List<LedgerLineDto>>
    {
        private readonly ILedgerLineRepository _repo;

        public GetLedgerLinesByExpenseQueryHandler(ILedgerLineRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<LedgerLineDto>> Handle(GetLedgerLinesByExpenseQuery request, CancellationToken ct)
        {
            var lines = await _repo.GetListAsync(l => l.ExpenseId == request.ExpenseId);

            // Sistem alanlarını sızdırmadan map’liyoruz
            return lines
                .Where(l => l.IsActive)
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
