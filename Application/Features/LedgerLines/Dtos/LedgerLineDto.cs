using System;

namespace Application.Features.LedgerLines.Dtos
{
    public class LedgerLineDto
    {
        public long Id { get; set; }          // long: cast hatalarını önlemek için
        public long HouseId { get; set; }
        public long ExpenseId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PostDate { get; set; }
    }
}
