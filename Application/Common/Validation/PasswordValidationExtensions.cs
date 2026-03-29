using FluentValidation;

namespace Application.Common.Validation;

public static class PasswordValidationExtensions
{
    private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,64}$";

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Sifre zorunludur.")
            .Matches(PasswordPattern)
            .WithMessage("Sifre en az 8 karakter olmali; buyuk harf, kucuk harf, rakam ve ozel karakter icermelidir.");
    }
}
