// Application/Features/Auths/Commands/VerifyCodeAndRegister/VerifyCodeAndRegisterCommand.cs
using MediatR;
using Application.Features.Auths.Dtos;

namespace Application.Features.Auths.Commands.VerifyCodeAndRegister
{
    public class VerifyCodeAndRegisterCommand
        : IRequest<VerifyCodeAndRegisterResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Davet linki üzerinden gelen kayıtlarda doldurulur.
        /// Dolu ise kullanıcı kayıt sonrası otomatik olarak ilgili eve eklenir.
        /// </summary>
        public string? InvitationToken { get; set; }
    }
}
