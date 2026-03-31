using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class Share
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }
        public int HarcamaId { get; set; }
        public Expense Expense { get; set; } = null!;

        public int UserId { get; set; }
        public int PaylasimUserId { get; set; }
        public User User { get; set; } = null!;

        public decimal PaylasimTutar { get; set; }
        public PaylasimTuru PaylasimTuru { get; set; } // enum türünde zorunlu alan
        public DateTime Date { get; set; }

    }
}
