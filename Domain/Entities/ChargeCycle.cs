namespace Domain.Entities
{
    public enum ChargeCycleStatus { Open = 0, AwaitingBill = 1, Collecting = 2, Funded = 3, Paid = 4, Overdue = 5, Canceled = 6 }

    public class ChargeCycle
    {
        public int Id { get; set; }                 // ⬅️ BaseEntity yerine doğrudan Id
        public int ContractId { get; set; }
        public RecurringCharge Contract { get; set; } = null!;

        public string Period { get; set; } = "";    // "YYYY-MM"
        public ChargeCycleStatus Status { get; set; }

        public decimal TotalAmount { get; set; }    // Fixed: contract.FixedAmount; Variable: SetBill ile
        public decimal FundedAmount { get; set; }   // Approved katkıların toplamı (denormalize)

        public DateTime? BillDate { get; set; }     // Variable için
        public string? BillNumber { get; set; }
        public string? BillDocumentUrl { get; set; }

        public DateTime? DueDate { get; set; }      // Fixed: period+DueDay; Variable: BillDate + N gün
        public DateTime? PaidDate { get; set; }
        public string? ExternalReceiptUrl { get; set; }
    }
}
