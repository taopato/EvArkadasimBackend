using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public partial class Expense
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty; // “Tur”
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }
        public bool IsActive { get; set; } = true;

        public int HouseId { get; set; }
        public House House { get; set; } = null!;

        public int OdeyenUserId { get; set; }
        public User OdeyenUser { get; set; } = null!;

        public int KaydedenUserId { get; set; }
        public User KaydedenUser { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<PersonalExpense> PersonalExpenses { get; set; } = new List<PersonalExpense>();
        public ICollection<Share> Shares { get; set; } = new List<Share>();

        // --- Yeni alanlar (plan / taksit metadata) ---
        /// <summary>Self-reference: Taksite/plan çocukları için üst kayıt.</summary>
        public int? ParentExpenseId { get; set; }
        public Expense? ParentExpense { get; set; }
        public ICollection<Expense> Children { get; set; } = new List<Expense>();

        /// <summary>Vade günü (1–28). Recurring/taksitli çocukların gününü belirler.</summary>
        public byte? DueDay { get; set; }

        /// <summary>Plan başlangıç ayı (UTC, ayın 1’i 00:00). Görsel/rapor amaçlı.</summary>
        public DateTime? PlanStartMonth { get; set; }

        /// <summary>Taksitli çocuklar için sıra (1-based).</summary>
        public int? InstallmentIndex { get; set; }

        /// <summary>Taksitli plan toplam taksit sayısı.</summary>
        public int? InstallmentCount { get; set; }

        // --- Serbest not alanı ---
        [MaxLength(512)]
        public string? Note { get; set; }

        // --- Alias'lar (DB'ye yazılmaz) ---
        [NotMapped]
        public string Tur
        {
            get => Description;
            set => Description = value ?? string.Empty;
        }

        [NotMapped]
        public DateTime KayitTarihi
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }
    }
}
