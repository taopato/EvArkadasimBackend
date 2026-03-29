using System;

namespace Application.Features.Payments.Commands.RejectPayment
{
    public class RejectPaymentResultDto
    {
        public int PaymentId { get; set; }
        public DateTime RejectedDate { get; set; }
        public int RejectedByUserId { get; set; }
    }
}
