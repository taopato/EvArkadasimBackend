namespace Domain.Entities
{
    public enum ChargeCycleShareStatus
    {
        Unpaid = 0,
        Paid = 1,
        Overdue = 2
    }

    public class ChargeCycleShare
    {
        public int Id { get; set; }
        public int ChargeCycleId { get; set; }
        public ChargeCycle ChargeCycle { get; set; } = null!;

        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public ChargeCycleShareStatus Status { get; set; } = ChargeCycleShareStatus.Unpaid;
        public DateTime? PaidDate { get; set; }
        public int? ConfirmedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
