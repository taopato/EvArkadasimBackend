// Application/Features/Auths/Commands/SendVerificationCode/SendVerificationCodeCommand.cs
using Application.Features.Auths.Dtos;
using MediatR;

namespace Application.Features.Auths.Commands.SendVerificationCode
{
    public class SendVerificationCodeCommand : IRequest<SendVerificationCodeResponseDto>
    {
        public string Email { get; set; } = string.Empty;
    }
}
