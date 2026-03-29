using Application.Features.Houses.Dtos;
using MediatR;

namespace Application.Features.Houses.Commands.CreateHouse
{
    public class CreateHouseCommand : IRequest<CreatedHouseDto>
    {
        public string Name { get; set; } = string.Empty;
        public int CreatorUserId { get; set; }
    }
}
