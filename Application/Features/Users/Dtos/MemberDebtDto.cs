// Application/Features/Houses/Dtos/MemberDebtDto.cs
namespace Application.Features.Houses.Dtos
{
    public class MemberDebtDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Alacak { get; set; }
        public decimal Borc { get; set; }
        
    }
}
