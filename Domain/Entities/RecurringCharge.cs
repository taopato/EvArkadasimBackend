namespace Domain.Entities
{
    public enum ChargeType { Rent = 0, Internet = 1, Electric = 2, Water = 3, Gas = 4, Other = 99 }
    public enum AmountMode { Fixed = 0, Variable = 1 }
    public enum SplitPolicy { Equal = 0, Weight = 1 }
    public enum RecurringAccountingMode { ScheduledCollection = 0, RunningBalance = 1 }

    public class RecurringCharge
    {
        public int Id { get; set; }                 // ⬅️ BaseEntity yerine doğrudan Id
        public int HouseId { get; set; }
        public ChargeType Type { get; set; }
        public string Title { get; set; } = string.Empty;

        public int PayerUserId { get; set; }
        public int CreatedByUserId { get; set; }

        public AmountMode AmountMode { get; set; }
        public decimal? FixedAmount { get; set; }   // Fixed ise zorunlu

        public SplitPolicy SplitPolicy { get; set; }
        public string? WeightsJson { get; set; }    // Weight için {userId:weight} JSON
        public string? ParticipantsJson { get; set; }

        public int? DueDay { get; set; }            // Fixed için vade günü (1–28)
        public int CollectionStartDay { get; set; } = 10;
        public int PaymentWindowDays { get; set; } = 5; // Variable için fatura sonrası gün
        public RecurringAccountingMode AccountingMode { get; set; } = RecurringAccountingMode.ScheduledCollection;

        public string StartMonth { get; set; } = ""; // "YYYY-MM"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChargeCycle> Cycles { get; set; } = new List<ChargeCycle>();
    }
}
