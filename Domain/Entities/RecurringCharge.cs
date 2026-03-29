namespace Domain.Entities
{
    public enum ChargeType { Rent = 0, Internet = 1, Electric = 2, Water = 3, Other = 99 }
    public enum AmountMode { Fixed = 0, Variable = 1 }
    public enum SplitPolicy { Equal = 0, Weight = 1 }

    public class RecurringCharge
    {
        public int Id { get; set; }                 // ⬅️ BaseEntity yerine doğrudan Id
        public int HouseId { get; set; }
        public ChargeType Type { get; set; }

        public int PayerUserId { get; set; }

        public AmountMode AmountMode { get; set; }
        public decimal? FixedAmount { get; set; }   // Fixed ise zorunlu

        public SplitPolicy SplitPolicy { get; set; }
        public string? WeightsJson { get; set; }    // Weight için {userId:weight} JSON

        public int? DueDay { get; set; }            // Fixed için vade günü (1–28)
        public int PaymentWindowDays { get; set; } = 5; // Variable için fatura sonrası gün

        public string StartMonth { get; set; } = ""; // "YYYY-MM"
        public bool IsActive { get; set; } = true;

        public ICollection<ChargeCycle> Cycles { get; set; } = new List<ChargeCycle>();
    }
}
