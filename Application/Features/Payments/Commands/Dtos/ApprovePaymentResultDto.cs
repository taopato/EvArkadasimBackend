using System;

namespace Application.Features.Payments.Commands.ApprovePayment
{
    public class ApprovePaymentResultDto
    {
        public int PaymentId { get; set; }
        public DateTime ApprovedDate { get; set; }
        public int ApprovedByUserId { get; set; }
    }
}
