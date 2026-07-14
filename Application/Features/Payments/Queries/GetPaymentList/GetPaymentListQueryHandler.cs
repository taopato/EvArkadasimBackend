using MediatR;
using Application.Features.Payments.Dtos;
using Application.Services.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Payments.Queries.GetPaymentList
{
    public class GetPaymentListQueryHandler
        : IRequestHandler<GetPaymentListQuery, List<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepo;

        public GetPaymentListQueryHandler(IPaymentRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<List<PaymentDto>> Handle(
            GetPaymentListQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _paymentRepo.GetByHouseIdAsync(request.HouseId);
            return list.Select(payment => new PaymentDto
            {
                Id = payment.Id,
                HouseId = payment.HouseId,
                BorcluUserId = payment.BorcluUserId,
                AlacakliUserId = payment.AlacakliUserId,
                Tutar = payment.Tutar,
                CreatedDate = payment.CreatedDate,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                BorcluUserName = payment.BorcluUser is null
                    ? string.Empty
                    : $"{payment.BorcluUser.FirstName} {payment.BorcluUser.LastName}".Trim(),
                AlacakliUserName = payment.AlacakliUser is null
                    ? string.Empty
                    : $"{payment.AlacakliUser.FirstName} {payment.AlacakliUser.LastName}".Trim(),
                Aciklama = payment.Aciklama,
                OdemeTarihi = payment.OdemeTarihi
            }).ToList();
        }
    }
}
