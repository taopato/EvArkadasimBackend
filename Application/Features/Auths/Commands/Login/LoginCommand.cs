using MediatR;
using Application.Features.Auths.Dtos;

namespace Application.Features.Auths.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}