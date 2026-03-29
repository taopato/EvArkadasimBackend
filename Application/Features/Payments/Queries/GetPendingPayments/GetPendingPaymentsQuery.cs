using System.Collections.Generic;
using MediatR;

namespace Application.Features.Payments.Queries.GetPendingPayments
{
    public class GetPendingPaymentsQuery : IRequest<IList<PendingPaymentDto>>
    {
        public int UserId { get; set; } // Alacaklı kullanıcı (payer)
        public GetPendingPaymentsQuery(int userId) => UserId = userId;
    }
}
