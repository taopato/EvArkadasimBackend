// Application/Features/Expenses/Queries/GetExpense/GetExpenseQuery.cs
using MediatR;
using Application.Features.Expenses.Dtos;

namespace Application.Features.Expenses.Queries.GetExpense
{
    public class GetExpenseQuery : IRequest<ExpenseDetailDto>
    {
        public int ExpenseId { get; set; }
    }
}
