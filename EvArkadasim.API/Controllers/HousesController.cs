using Application.Features.Houses.Commands.CreateHouse;
using Application.Features.Houses.Queries.GetHouseMembersWithDebts;
using Application.Features.Houses.Queries.GetUserDebtBetween;
using Application.Features.Houses.Queries.GetUserDebts;
using Application.Features.Houses.Queries.GetUserHouses;
using Application.Features.Invitations.Commands.SendInvitation;
using Application.Features.Invitations.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HousesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HousesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHouseCommand command)
        {
            var dto = await _mediator.Send(command);
            return Ok(dto);
        }

        [HttpPost("{houseId}/invitations")]
        public async Task<IActionResult> SendInvitation(int houseId, [FromBody] SendInvitationRequestDto request)
        {
            var command = new SendInvitationCommand
            {
                HouseId = houseId,
                Email = request.Email
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{houseId}/members")]
        public async Task<IActionResult> GetHouseMembers(int houseId)
        {
            var result = await _mediator.Send(new GetHouseMembersWithDebtsQuery { HouseId = houseId });
            return Ok(result);
        }

        [HttpGet("GetUserHouses/{userId}")]
        public async Task<IActionResult> GetUserHouses(int userId)
        {
            var result = await _mediator.Send(new GetUserHousesQuery { UserId = userId });
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("GetUserDebts/{userId:int}/{houseId:int}")]
        public async Task<IActionResult> GetUserDebts(int userId, int houseId)
        {
            var result = await _mediator.Send(new GetUserDebtsQuery(userId, houseId));
            return Ok(result);
        }

        [HttpGet("GetUserDebtBetween/{houseId:int}")]
        public async Task<IActionResult> GetUserDebtBetween(int houseId, [FromQuery] int userAId, [FromQuery] int userBId)
        {
            var result = await _mediator.Send(new GetUserDebtBetweenQuery
            {
                HouseId = houseId,
                UserAId = userAId,
                UserBId = userBId
            });

            return Ok(result);
        }
    }
}
