using FluentValidation;

namespace Application.Features.Auths.Commands.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Gecerli bir e-posta giriniz.")
            .MaximumLength(200);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MinimumLength(2).WithMessage("Ad soyad en az 2 karakter olmali.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");
    }
}
