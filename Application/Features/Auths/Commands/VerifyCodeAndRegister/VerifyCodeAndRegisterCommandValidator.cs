using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Auths.Commands.VerifyCodeAndRegister;

public class VerifyCodeAndRegisterCommandValidator : AbstractValidator<VerifyCodeAndRegisterCommand>
{
    public VerifyCodeAndRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Gecerli bir e-posta giriniz.")
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dogrulama kodu zorunludur.")
            .Matches(@"^\d{6}$").WithMessage("Dogrulama kodu 6 haneli olmali.");

        When(x => !string.IsNullOrWhiteSpace(x.FullName), () =>
        {
            RuleFor(x => x.FullName!)
                .NotEmpty().WithMessage("Ad soyad zorunludur.")
                .MinimumLength(3).WithMessage("Ad soyad en az 3 karakter olmali.")
                .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

            RuleFor(x => x.Password).StrongPassword();
        });
    }
}
