using MediatR;
using Application.Features.Users.Dtos;

namespace Application.Features.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand(
        int UserId,
        string? FullName,
        string? PhoneNumber,
        string? Iban,
        string? ProfileImageUrl,
        string? CurrentPassword,
        string? NewPassword
    ) : IRequest<UpdateProfileResponseDto>;
}
