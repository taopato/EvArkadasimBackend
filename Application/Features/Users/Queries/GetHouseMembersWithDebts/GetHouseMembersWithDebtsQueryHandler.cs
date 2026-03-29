using AutoMapper;
using MediatR;
using Application.Services.Repositories;
using Application.Features.Houses.Dtos;

namespace Application.Features.Houses.Queries.GetHouseMembersWithDebts
{
    public class GetHouseMembersWithDebtsQueryHandler
        : IRequestHandler<GetHouseMembersWithDebtsQuery, List<MemberDebtDto>>
    {
        private readonly IHouseRepository _houseRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IMapper _mapper;

        public GetHouseMembersWithDebtsQueryHandler(
            IHouseRepository houseRepo,
            IPaymentRepository paymentRepo,
            IMapper mapper)
        {
            _houseRepo = houseRepo;
            _paymentRepo = paymentRepo;
            _mapper = mapper;
        }

        public async Task<List<MemberDebtDto>> Handle(
            GetHouseMembersWithDebtsQuery request,
            CancellationToken cancellationToken)
        {
            // 1) Ev üyelerini al
            var house = await _houseRepo.GetByIdAsync(request.HouseId);

            // 2) Her üye için alacak ve borç tutarlarını hesapla
            var result = new List<MemberDebtDto>();
            foreach (var member in house.HouseMembers)
            {
                var uid = member.UserId;
                var alacak = await _paymentRepo.GetTotalAlacaklıAsync(request.HouseId, uid);
                var borc = await _paymentRepo.GetTotalBorçluAsync(request.HouseId, uid);

                result.Add(new MemberDebtDto
                {
                    UserId = uid,
                    FullName = $"{member.User.FirstName} {member.User.LastName}",
                    Email = member.User.Email,
                    Alacak = alacak,
                    Borc = borc
                });
            }

            return result;
        }
    }
}