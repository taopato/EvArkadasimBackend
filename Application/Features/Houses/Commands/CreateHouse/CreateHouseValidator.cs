using FluentValidation;

namespace Application.Features.Houses.Commands.CreateHouse
{
    public class CreateHouseValidator : AbstractValidator<CreateHouseCommand>
    {
        public CreateHouseValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Ev grubu adı boş olamaz.");
            RuleFor(x => x.CreatorUserId).GreaterThan(0);
        }
    }
}
