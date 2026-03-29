using FluentValidation;

namespace Application.Features.Auths.Commands.SendVerificationCode;

public class SendVerificationCodeCommandValidator : AbstractValidator<SendVerificationCodeCommand>
{
    public SendVerificationCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Gecerli bir e-posta giriniz.")
            .MaximumLength(200);

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Dogrulama amaci zorunludur.")
            .Must(x => x is "register" or "reset")
            .WithMessage("Dogrulama amaci gecersiz.");
    }
}
