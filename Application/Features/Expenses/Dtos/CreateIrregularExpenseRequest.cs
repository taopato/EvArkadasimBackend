// ... diğer using'ler
using Domain.Enums;

namespace Application.Features.Expenses.Dtos
{
    public class CreateIrregularExpenseRequest
    {
        public string Tur { get; set; } = string.Empty;
        public ExpenseCategory Category { get; set; }
        public decimal Tutar { get; set; }
        public int HouseId { get; set; }
        public int OdeyenUserId { get; set; }
        public int KaydedenUserId { get; set; }
        public DateTime? PostDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Note { get; set; }
        public List<PersonalItemDto>? PersonalItems { get; set; } = new();

        // ❌ default KALDIRILDI — enum’un 0 değeri neyse o gelir.
        public PaylasimTuru SplitPolicy { get; set; }
    }
}
