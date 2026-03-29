using FluentValidation;

namespace Application.Features.Expenses.Commands.CreateIrregularExpense
{
    public class CreateIrregularExpenseValidator : AbstractValidator<CreateIrregularExpenseCommand>
    {
        public CreateIrregularExpenseValidator()
        {
            RuleFor(x => x.Model).NotNull();

            RuleFor(x => x.Model.Tur).NotEmpty();
            RuleFor(x => x.Model.Tutar).GreaterThan(0);
            RuleFor(x => x.Model.HouseId).GreaterThan(0);
            RuleFor(x => x.Model.OdeyenUserId).GreaterThan(0);
            RuleFor(x => x.Model.KaydedenUserId).GreaterThan(0);
        }
    }
}
