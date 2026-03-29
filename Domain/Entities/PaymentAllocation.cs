namespace Domain.Entities
{
    public class PaymentAllocation
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        // LedgerLineId bizde long
        public long LedgerLineId { get; set; }
        public LedgerLine LedgerLine { get; set; } = null!;

        public decimal Amount { get; set; }

        // ⬇️ EKLENDİ: DB'deki NOT NULL kolona karşılık
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
