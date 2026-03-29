using MediatR;
using System.Collections.Generic;
using Application.Features.Houses.Dtos;

namespace Application.Features.Houses.Queries.GetHouseMembersWithDebts
{
    public class GetHouseMembersWithDebtsQuery : IRequest<List<MemberDebtDto>>
    {
        public int HouseId { get; set; }
    }
}