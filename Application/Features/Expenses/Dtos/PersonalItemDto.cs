namespace Application.Features.Expenses.Dtos
{
    public class PersonalItemDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; } // kişisel harcama
    }
}
