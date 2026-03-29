using MediatR;
using Application.Features.Expenses.Dtos;

namespace Application.Features.Expenses.Commands.UpdateExpense
{
    public class UpdateExpenseCommand : IRequest<Unit>
    {
        public int ExpenseId { get; set; }
        public UpdateExpenseDto Dto { get; set; } = null!;
    }
}