// Application/Features/Auths/Commands/ResetPassword/ResetPasswordValidator.cs
using FluentValidation;

namespace Application.Features.Auths.Commands.ResetPassword
{
    public class ResetPasswordValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code).NotEmpty().Length(6);
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage("Yeni şifre en az 8 karakter olmalı.");
        }
    }
}
