// Application/Features/Auths/Commands/ResetPassword/ResetPasswordValidator.cs
using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Auths.Commands.ResetPassword
{
    public class ResetPasswordValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code)
                .NotEmpty()
                .Matches(@"^\d{6}$")
                .WithMessage("Dogrulama kodu 6 haneli olmali.");
            RuleFor(x => x.NewPassword).StrongPassword();
        }
    }
}
