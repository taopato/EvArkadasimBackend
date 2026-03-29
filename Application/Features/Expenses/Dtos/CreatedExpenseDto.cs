namespace Application.Features.Expenses.Dtos
{
    public class CreatedExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OrtakHarcamaTutari { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
