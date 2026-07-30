// Application/Features/Auths/Dtos/LoginResponseDto.cs
namespace Application.Features.Auths.Dtos
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Iban { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Message { get; set; } = "Giriş başarılı!";
    }
}
