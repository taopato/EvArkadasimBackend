// Application/Features/Auths/Commands/ResetPassword/ResetPasswordCommand.cs
using MediatR;
using Application.Features.Auths.Dtos;

namespace Application.Features.Auths.Commands.ResetPassword
{
    public class ResetPasswordCommand
        : IRequest<ResetPasswordResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
