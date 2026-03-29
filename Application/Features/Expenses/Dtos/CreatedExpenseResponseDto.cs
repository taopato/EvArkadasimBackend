using System;
using System.Collections.Generic;

namespace Application.Features.Expenses.Dtos
{
    public class CreatedExpenseResponseDto
    {
        public int Id { get; set; }
        public string Tur { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }
        public string Message { get; set; } = "Harcama başarıyla eklendi.";
        public List<PersonalExpenseDto> PersonalExpenses { get; set; } = new();
        public List<ShareDto> Shares { get; set; } = new();
        public int HouseId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int OdeyenUserId { get; set; }
        public int KaydedenUserId { get; set; }
        public DateTime KayitTarihi { get; set; }

        // 🔹 Plan/Taksit metadata (opsiyonel ama FE için faydalı)
        public int? ParentExpenseId { get; set; }
        public int? InstallmentIndex { get; set; }
        public int? InstallmentCount { get; set; }
        public DateTime? PlanStartMonth { get; set; }
        public byte? DueDay { get; set; }
    }
}
