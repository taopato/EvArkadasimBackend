namespace Application.Features.Auths.Dtos
{
    public class VerifyCodeAndRegisterResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}