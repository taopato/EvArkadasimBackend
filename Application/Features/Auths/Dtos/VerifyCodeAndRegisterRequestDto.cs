// Application/Features/Auths/Dtos/VerifyCodeAndRegisterRequestDto.cs
namespace Application.Features.Auths.Dtos
{
    public class VerifyCodeAndRegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? FullName { get; set; } // nullable: yoksa reset kod doğrulama
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Davet linki üzerinden gelen kayıtlarda doldurulur. Code yerine bu kullanılır.
        /// </summary>
        public string? InvitationToken { get; set; }
    }
}
