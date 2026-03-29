namespace Application.Features.Auths.Dtos
{
    public class VerifyCodeAndRegisterResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Token { get; set; }
    }
}
