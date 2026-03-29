using Application.Features.Houses.Commands.CreateHouse;
using Application.Features.Houses.Commands.LeaveHouse;
using Application.Features.Houses.Queries.GetHouseMembersWithDebts;
using Application.Features.Houses.Queries.GetUserDebtBetween;
using Application.Features.Houses.Queries.GetUserDebts;
using Application.Features.Houses.Queries.GetUserHouses;
using Application.Features.Invitations.Commands.SendInvitation;
using Application.Features.Invitations.Dtos;
using Application.Services.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HousesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHouseRepository _houseRepository;

        public HousesController(IMediator mediator, IHouseRepository houseRepository)
        {
            _mediator = mediator;
            _houseRepository = houseRepository;
        }

        [HttpGet("{houseId:int}")]
        public async Task<IActionResult> GetById(int houseId)
        {
            try
            {
                var house = await _houseRepository.GetByIdAsync(houseId);
                return Ok(new
                {
                    house.Id,
                    house.Name,
                    house.CreatorUserId
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Ev bulunamadi." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHouseCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Unauthorized(new { message = "Gecerli kullanici kimligi bulunamadi." });
            }

            command.CreatorUserId = userId;
            var dto = await _mediator.Send(command);
            return Ok(dto);
        }

        [HttpPost("{houseId}/invitations")]
        public async Task<IActionResult> SendInvitation(int houseId, [FromBody] SendInvitationRequestDto request)
        {
            var command = new SendInvitationCommand { HouseId = houseId, Email = request.Email };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{houseId}/members")]
        public async Task<IActionResult> GetHouseMembers(int houseId)
        {
            var result = await _mediator.Send(new GetHouseMembersWithDebtsQuery { HouseId = houseId });
            return Ok(result);
        }

        /// <summary>DELETE /api/Houses/{houseId}/members/{userId}?requestingUserId=123 — Soft delete</summary>
        [HttpDelete("{houseId}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int houseId, int userId, [FromQuery] int requestingUserId)
        {
            await _mediator.Send(new LeaveHouseCommand
            {
                HouseId = houseId,
                UserId = userId,
                RequestingUserId = requestingUserId
            });
            return Ok(new { success = true, message = "Üye evden çıkarıldı." });
        }

        [HttpGet("GetUserHouses/{userId}")]
        public async Task<IActionResult> GetUserHouses(int userId)
        {
            var result = await _mediator.Send(new GetUserHousesQuery { UserId = userId });
            if (!result.Success) return BadRequest(result.Message);
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
            var result = await _mediator.Send(new GetUserDebtBetweenQuery { HouseId = houseId, UserAId = userAId, UserBId = userBId });
            return Ok(result);
        }
    }
}
