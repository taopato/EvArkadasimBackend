namespace Application.Features.Auths.Dtos
{
    public class AppleLoginRequestDto
    {
        public string IdentityToken { get; set; } = string.Empty;
        // Apple yalnızca ilk girişte gönderir; sonraki girişlerde boş olabilir.
        public string? FullName { get; set; }
    }
}
