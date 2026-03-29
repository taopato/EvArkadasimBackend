using MediatR;
using Application.Features.Expenses.Dtos;

namespace Application.Features.Expenses.Commands.CreateIrregularExpense
{
    public class CreateIrregularExpenseCommand : IRequest<CreatedExpenseSummaryDto>
    {
        public CreateIrregularExpenseRequest Model { get; set; } = null!;
    }
}
