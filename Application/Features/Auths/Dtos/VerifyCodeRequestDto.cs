// Application/Features/Auths/Dtos/VerifyCodeRequestDto.cs
namespace Application.Features.Auths.Dtos
{
    public class VerifyCodeRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
