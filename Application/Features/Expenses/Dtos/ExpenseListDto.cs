using System;

namespace Application.Features.Expenses.Dtos
{
    public class ExpenseListDto
    {
        public int Id { get; set; }
        public string Tur { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public int HouseId { get; set; }
        public int OdeyenUserId { get; set; }
        public int KaydedenUserId { get; set; }
        public DateTime KayitTarihi { get; set; }
        public string OdeyenKullaniciAdi { get; set; } = string.Empty;
        public string KaydedenKullaniciAdi { get; set; } = string.Empty;

        // 🔹 FE'nin notu okuyabilmesi için description alanı
        public string Description { get; set; } = string.Empty;

        // 🔹 Plan/Taksit metadata (FE için gerekli)
        public int? ParentExpenseId { get; set; }
        public int? InstallmentIndex { get; set; }     // 1-based
        public int? InstallmentCount { get; set; }
        public DateTime? PlanStartMonth { get; set; }  // ayın 1'i UTC
        public byte? DueDay { get; set; }              // 1–28
    }
}
