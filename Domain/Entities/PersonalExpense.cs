using System;

namespace Domain.Entities
{
    public class PersonalExpense
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }
        public Expense Expense { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal Tutar { get; set; }
    }
}
