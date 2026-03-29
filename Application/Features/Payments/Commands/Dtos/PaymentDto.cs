namespace Application.Features.Payments.Dtos
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int HouseId { get; set; }
        public int BorcluUserId { get; set; }
        public int AlacakliUserId { get; set; }
        public decimal Tutar { get; set; }
        public DateTime CreatedDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty; // BankTransfer, Cash gibi
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
        public string BorcluUserName { get; set; } = string.Empty; // Kullanıcı adı-soyadı
        public string AlacakliUserName { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public DateTime OdemeTarihi { get; set; }
    }
}
