using MediatR;

namespace Application.Features.Expenses.Commands.DeleteExpense
{
    public class DeleteExpenseCommand : IRequest<Unit>
    {
        public int ExpenseId { get; set; }
    }
}