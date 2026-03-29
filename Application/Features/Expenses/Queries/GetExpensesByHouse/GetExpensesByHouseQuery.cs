using MediatR;
using Application.Features.Expenses.Dtos;
using System.Collections.Generic;

namespace Application.Features.Expenses.Queries.GetExpensesByHouse
{
    public class GetExpensesByHouseQuery : IRequest<List<ExpenseListDto>>
    {
        public int HouseId { get; set; }
    }
}