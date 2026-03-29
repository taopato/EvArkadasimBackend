// Application/Features/Auths/Dtos/AuthResultDto.cs
namespace Application.Features.Auths.Dtos
{
    public class AuthResultDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
