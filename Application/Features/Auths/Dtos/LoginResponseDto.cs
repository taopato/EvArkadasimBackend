// Application/Features/Auths/Dtos/LoginResponseDto.cs
namespace Application.Features.Auths.Dtos
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = "Giriş başarılı!";
    }
}
