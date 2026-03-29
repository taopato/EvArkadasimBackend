using MediatR;

namespace Application.Features.Payments.Commands.DeletePayment
{
    public class DeletePaymentCommand : IRequest<Unit>
    {
        public int PaymentId { get; set; }
        public int RequestingUserId { get; set; }

        public DeletePaymentCommand(int paymentId, int requestingUserId)
        {
            PaymentId = paymentId;
            RequestingUserId = requestingUserId;
        }
    }
}
