using System;
using Domain.Enums;

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
        public string Description { get; set; } = string.Empty;
        public int? ParentExpenseId { get; set; }
        public int? InstallmentIndex { get; set; }
        public int? InstallmentCount { get; set; }
        public DateTime? PlanStartMonth { get; set; }
        public byte? DueDay { get; set; }
        public DateTime? PostDate { get; set; }
        public DateTime? DueDate { get; set; }
        public short? PreShareDays { get; set; }
        public VisibilityMode VisibilityMode { get; set; }
        public ExpenseCategory Category { get; set; }
    }
}
