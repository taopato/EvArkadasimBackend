using Application.Features.Auths.Dtos;
using MediatR;

namespace Application.Features.Auths.Commands.GoogleLogin
{
    public class GoogleLoginCommand : IRequest<LoginResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}

