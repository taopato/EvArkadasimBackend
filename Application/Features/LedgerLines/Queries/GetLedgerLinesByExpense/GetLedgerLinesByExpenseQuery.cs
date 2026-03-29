using System.Collections.Generic;
using Application.Features.LedgerLines.Dtos;
using MediatR;

namespace Application.Features.LedgerLines.Queries.GetLedgerLinesByExpense
{
    public class GetLedgerLinesByExpenseQuery : IRequest<List<LedgerLineDto>>
    {
        public int ExpenseId { get; set; }
    }
}
