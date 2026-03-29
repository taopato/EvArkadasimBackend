using MediatR;

namespace Application.Features.Payments.Commands.RejectPayment
{
    public class RejectPaymentCommand : IRequest<RejectPaymentResultDto>
    {
        public int PaymentId { get; set; }
        public RejectPaymentCommand(int paymentId) => PaymentId = paymentId;
    }
}
