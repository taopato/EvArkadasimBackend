namespace Application.Features.Users.Dtos
{
    public class UpdateProfileResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Iban { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
