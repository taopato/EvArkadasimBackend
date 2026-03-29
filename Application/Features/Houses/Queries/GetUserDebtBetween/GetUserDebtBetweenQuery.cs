using Application.Features.Houses.Dtos;
using MediatR;

namespace Application.Features.Houses.Queries.GetUserDebtBetween
{
    public class GetUserDebtBetweenQuery : IRequest<PairDebtDetailDto>
    {
        public int HouseId { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
    }
}
