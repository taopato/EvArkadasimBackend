public class CreatePaymentRequest
{
    public int HouseId { get; set; }
    public int BorcluUserId { get; set; }
    public int AlacakliUserId { get; set; }
    public decimal Tutar { get; set; }
    public string PaymentMethod { get; set; } = "BankTransfer";
    public string Note { get; set; } = ""; // NOT NULL olmalı
    public int? ChargeId { get; set; }
}
