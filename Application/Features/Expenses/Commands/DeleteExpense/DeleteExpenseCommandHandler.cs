using MediatR;
using Application.Services.Repositories;

namespace Application.Features.Expenses.Commands.DeleteExpense
{
    public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Unit>
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly ILedgerLineRepository _ledgerRepo;

        public DeleteExpenseCommandHandler(IExpenseRepository expenseRepo, ILedgerLineRepository ledgerRepo)
        {
            _expenseRepo = expenseRepo;
            _ledgerRepo = ledgerRepo;
        }

        public async Task<Unit> Handle(DeleteExpenseCommand request, CancellationToken ct)
        {
            var e = await _expenseRepo.GetByIdAsync(request.ExpenseId)
                    ?? throw new KeyNotFoundException("Expense not found");

            var paidLines = await _ledgerRepo.GetListAsync(
                line => line.ExpenseId == e.Id && line.IsActive && line.PaidAmount > 0m,
                ct);
            if (paidLines.Any())
            {
                throw new InvalidOperationException(
                    "Bu harcamaya ait ödeme bulunduğu için kayıt silinemez.");
            }

            // SOFT DELETE
            e.IsActive = false;
            e.UpdatedAt = DateTime.UtcNow;
            await _expenseRepo.UpdateAsync(e);
            await _expenseRepo.SaveChangesAsync();

            // Ledger satırlarını da pasif yap (varsa)
            await _ledgerRepo.SoftDeleteByExpenseIdAsync(e.Id, ct);
            await _ledgerRepo.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
