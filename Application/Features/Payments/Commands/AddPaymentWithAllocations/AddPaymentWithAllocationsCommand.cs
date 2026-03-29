using MediatR;

namespace Application.Features.Payments.Commands.AddPaymentWithAllocations
{
    public class AddPaymentWithAllocationsCommand : IRequest<AddPaymentWithAllocationsResult>
    {
        public AddPaymentWithAllocationsDto Model { get; set; } = null!;
    }
}
