using MediatR;
using Application.Features.Payments.Dtos;
using System.Collections.Generic;

namespace Application.Features.Payments.Queries.GetPaymentList
{
    public class GetPaymentListQuery : IRequest<List<PaymentDto>>
    {
        public int HouseId { get; set; }
    }
}