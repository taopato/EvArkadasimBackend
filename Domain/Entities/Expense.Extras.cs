using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    // Expense ana sınıfının devamı (partial)
    public partial class Expense
    {
        // Düzenli/Düzensiz
        public ExpenseType Type { get; set; } = ExpenseType.Irregular;

        // Kategori (Kira/İnternet/Elektrik/Su/Market/Yemek/Diğer)
        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

        /// <summary>Borcun ekranda görünmeye başladığı gün (UTC).</summary>
        public DateTime PostDate { get; set; } = DateTime.UtcNow;

        /// <summary>Son ödeme günü (varsa, UTC).</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Raporlama için "YYYY-MM".</summary>
        [MaxLength(7)]
        public string PeriodMonth { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");

        /// <summary>Paylaşım kuralı: Eşit / Ağırlıklı / Kişisel.</summary>
        public PaylasimTuru SplitPolicy { get; set; } // default enum

        /// <summary>Ağırlıklı/katılımcı listesi: [{"userId":X,"weight":Y},...]</summary>
        public string? ParticipantsJson { get; set; }

        /// <summary>Kişisel kalemler: [{"userId":X,"amount":Y},...]</summary>
        public string? PersonalBreakdownJson { get; set; }

        /// <summary>Görünürlük modu.</summary>
        public VisibilityMode VisibilityMode { get; set; } = VisibilityMode.OnBillDate;

        /// <summary>Son ödeme gününden X gün önce gösterilecekse X.</summary>
        public short? PreShareDays { get; set; } = 0;

        /// <summary>Düzenli paket (kira/internet) 12 ay seti için ortak anahtar.</summary>
        public Guid? RecurrenceBatchKey { get; set; }

        /// <summary>Para birimi (varsayılan TRY).</summary>
        [MaxLength(3)]
        public string? CurrencyCode { get; set; } = "TRY";


        public DateTime? UpdatedAt { get; set; }
    }
}
