namespace Application.Features.Houses.Dtos
{
    public class PairExpenseItemDto
    {
        public int ExpenseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public System.DateTime Date { get; set; }
    }

    public class PairDebtDetailDto
    {
        public int HouseId { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public string UserAName { get; set; } = string.Empty;
        public string UserBName { get; set; } = string.Empty;
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public string BorcluUserName { get; set; } = string.Empty;
        public string AlacakliUserName { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Net { get; set; }
        public int NetForUserId { get; set; }
        public System.Collections.Generic.List<PairExpenseItemDto> RecentExpenses { get; set; } = new();
    }
}
