namespace Domain.Entities
{
    public class BillShare
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int UserId { get; set; }
        public decimal ShareAmount { get; set; } // 2 ondalık

        public Bill Bill { get; set; } = null!;
    }
}
