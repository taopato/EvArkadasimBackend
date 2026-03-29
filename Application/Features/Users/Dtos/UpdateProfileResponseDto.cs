namespace Application.Features.Users.Dtos
{
    public class UpdateProfileResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
