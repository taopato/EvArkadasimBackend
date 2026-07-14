using Domain.Entities;

namespace Application.Features.Expenses.Dtos
{
    internal static class ExpenseDtoMapper
    {
        public static ExpenseListDto ToListDto(Expense expense)
        {
            return new ExpenseListDto
            {
                Id = expense.Id,
                Tur = expense.Tur,
                Tutar = expense.Tutar,
                HouseId = expense.HouseId,
                OdeyenUserId = expense.OdeyenUserId,
                KaydedenUserId = expense.KaydedenUserId,
                KayitTarihi = expense.CreatedDate,
                Description = expense.Description,
                Note = expense.Note,
                ParentExpenseId = expense.ParentExpenseId,
                InstallmentIndex = expense.InstallmentIndex,
                InstallmentCount = expense.InstallmentCount,
                PlanStartMonth = expense.PlanStartMonth,
                DueDay = expense.DueDay,
                PostDate = expense.PostDate,
                DueDate = expense.DueDate,
                PreShareDays = expense.PreShareDays,
                VisibilityMode = expense.VisibilityMode,
                Category = expense.Category
            };
        }

        public static ExpenseDetailDto ToDetailDto(Expense expense)
        {
            return new ExpenseDetailDto
            {
                Id = expense.Id,
                Tur = expense.Tur,
                Tutar = expense.Tutar,
                HouseId = expense.HouseId,
                OdeyenUserId = expense.OdeyenUserId,
                KaydedenUserId = expense.KaydedenUserId,
                KayitTarihi = expense.CreatedDate,
                Description = expense.Description,
                Note = expense.Note,
                ParentExpenseId = expense.ParentExpenseId,
                InstallmentIndex = expense.InstallmentIndex,
                InstallmentCount = expense.InstallmentCount,
                PlanStartMonth = expense.PlanStartMonth,
                DueDay = expense.DueDay,
                PostDate = expense.PostDate,
                DueDate = expense.DueDate,
                PreShareDays = expense.PreShareDays,
                VisibilityMode = expense.VisibilityMode,
                Category = expense.Category,
                OrtakHarcamaTutari = expense.OrtakHarcamaTutari
            };
        }
    }
}
