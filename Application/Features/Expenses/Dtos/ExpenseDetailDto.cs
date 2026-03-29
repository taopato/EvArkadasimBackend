using System;
using System.Collections.Generic;

namespace Application.Features.Expenses.Dtos
{
    public class ExpenseDetailDto : ExpenseListDto
    {
        public List<PersonalExpenseDto> SahsiHarcamalar { get; set; } = new();
        public decimal OrtakHarcamaTutari { get; set; }

        // Projendeki mevcut yapı korunuyor
        public string Tur { get; set; } = string.Empty;
        public DateTime KayitTarihi { get; set; }
    }
}
