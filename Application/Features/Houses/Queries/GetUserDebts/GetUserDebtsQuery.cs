using Application.Features.Houses.Dtos;
using MediatR;

namespace Application.Features.Houses.Queries.GetUserDebts
{
    public class GetUserDebtsQuery : IRequest<GetUserDebtsDto>
    {
        public int HouseId { get; set; }
        public int? UserId { get; set; }

        public GetUserDebtsQuery() { }

        public GetUserDebtsQuery(int userId, int houseId)
        {
            UserId = userId;
            HouseId = houseId;
        }
    }
}
