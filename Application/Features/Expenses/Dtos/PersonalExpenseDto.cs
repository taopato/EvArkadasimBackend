// Application/Features/Expenses/Dtos/PersonalExpenseDto.cs
public class PersonalExpenseDto
{
    public int UserId { get; set; }
    public decimal Tutar { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;  // eklendi
}
