namespace Application.Features.Auths.Dtos
{
    public class ResetPasswordResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}