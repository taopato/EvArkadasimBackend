using Application.Features.Houses.Dtos;
using Core.Utilities.Results;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Houses.Queries.GetUserHouses
{
    public class GetUserHousesQuery : IRequest<Response<List<UserHouseListDto>>>
    {
        public int UserId { get; set; }
    }
}
