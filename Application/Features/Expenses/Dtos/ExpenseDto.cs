namespace Application.Features.Expenses.Dtos
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }
        public DateTime CreatedDate { get; set; }
        // ALIASES – mapping ve UI'nin beklediği alan adları
        public string Tur { get; set; } = string.Empty;
        public DateTime KayitTarihi { get; set; }
        public string OdeyenKullaniciAdi { get; set; } = string.Empty;
        public string KaydedenKullaniciAdi { get; set; } = string.Empty;
    }
}
