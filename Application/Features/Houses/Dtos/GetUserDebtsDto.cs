using System.Collections.Generic;

namespace Application.Features.Houses.Dtos
{
    public class GetUserDebtsDto
    {
        public int HouseId { get; set; }
        public List<PairDebtDto> Pairs { get; set; } = new();
        public List<UserTotalDto> Totals { get; set; } = new();

        public class PairDebtDto
        {
            public int FromUserId { get; set; }   // borçlu
            public int ToUserId { get; set; }     // alacaklı
            public decimal NetAmount { get; set; }
        }

        public class UserTotalDto
        {
            public int UserId { get; set; }
            public decimal Receivable { get; set; }
            public decimal Payable { get; set; }
            public decimal Net { get; set; } // Receivable - Payable
        }
    }
}
