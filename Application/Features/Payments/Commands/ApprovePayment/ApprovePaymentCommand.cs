using MediatR;

namespace Application.Features.Payments.Commands.ApprovePayment
{
    public class ApprovePaymentCommand : IRequest<ApprovePaymentResultDto>
    {
        public int PaymentId { get; set; }
        public ApprovePaymentCommand(int paymentId) => PaymentId = paymentId;
    }
}
