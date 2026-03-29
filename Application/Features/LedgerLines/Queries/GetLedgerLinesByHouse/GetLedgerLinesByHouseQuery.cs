using System.Collections.Generic;
using Application.Features.LedgerLines.Dtos;
using MediatR;

namespace Application.Features.LedgerLines.Queries.GetLedgerLinesByHouse
{
    public class GetLedgerLinesByHouseQuery : IRequest<List<LedgerLineDto>>
    {
        public int HouseId { get; set; }
    }
}
