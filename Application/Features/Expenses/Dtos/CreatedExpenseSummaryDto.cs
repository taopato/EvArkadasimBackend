using System;
using System.Collections.Generic;

namespace Application.Features.Expenses.Dtos
{
    public class CreatedExpenseSummaryDto
    {
        public int ExpenseId { get; set; }
        public int HouseId { get; set; }
        public int OdeyenUserId { get; set; }
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }
        public DateTime PostDate { get; set; }
        public string PeriodMonth { get; set; } = string.Empty;

        public List<LedgerItem> Ledger { get; set; } = new();

        public class LedgerItem
        {
            public int FromUserId { get; set; }
            public int ToUserId { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
