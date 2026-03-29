using MediatR;
using Application.Features.Payments.Dtos;
using Application.Services.Repositories;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Payments.Queries.GetPaymentList
{
    public class GetPaymentListQueryHandler
        : IRequestHandler<GetPaymentListQuery, List<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IMapper _mapper;

        public GetPaymentListQueryHandler(
            IPaymentRepository paymentRepo,
            IMapper mapper)
        {
            _paymentRepo = paymentRepo;
            _mapper = mapper;
        }

        public async Task<List<PaymentDto>> Handle(
            GetPaymentListQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _paymentRepo.GetByHouseIdAsync(request.HouseId);
            return _mapper.Map<List<PaymentDto>>(list);
        }
    }
}