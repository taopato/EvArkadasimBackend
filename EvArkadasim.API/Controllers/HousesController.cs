using Application.Features.Houses.Commands.CreateHouse;
using Application.Features.Houses.Commands.LeaveHouse;
using Application.Features.Houses.Queries.GetHouseMembersWithDebts;
using Application.Features.Houses.Queries.GetUserDebtBetween;
using Application.Features.Houses.Queries.GetUserDebts;
using Application.Features.Houses.Queries.GetUserHouses;
using Application.Features.Invitations.Commands.SendInvitation;
using Application.Features.Invitations.Dtos;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;

        public HousesController(
            IMediator mediator,
            IHouseRepository houseRepository,
            IInvitationRepository invitationRepository,
            IUserRepository userRepository)
        {
            _mediator = mediator;
            _houseRepository = houseRepository;
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
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

        [HttpPost("AcceptInvitation")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequestDto request)
        {
            var invitationCode = request.InvitationCode?.Trim();
            if (string.IsNullOrWhiteSpace(invitationCode))
            {
                return BadRequest(new { message = "Davet kodu zorunludur." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Unauthorized(new { message = "Gecerli kullanici kimligi bulunamadi." });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanici bulunamadi." });
            }

            var invitation = await _invitationRepository.GetByCodeAsync(invitationCode);
            if (invitation == null)
            {
                return BadRequest(new { message = "Gecersiz davet linki." });
            }

            if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Davet linkinin suresi dolmustur." });
            }

            if (!string.IsNullOrWhiteSpace(invitation.Email) &&
                !string.Equals(invitation.Email.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Bu davet farkli bir e-posta adresine gonderilmis." });
            }

            var alreadyMember = await _houseRepository.IsActiveMemberAsync(invitation.HouseId, userId);
            if (!alreadyMember)
            {
                if (!string.Equals(invitation.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Bu davet daha once kullanilmis." });
                }

                await _houseRepository.AddMemberAsync(new HouseMember
                {
                    HouseId = invitation.HouseId,
                    UserId = userId,
                    JoinedDate = DateTime.UtcNow,
                    IsActive = true
                });
            }

            invitation.Status = "Accepted";
            invitation.AcceptedByUserId = userId;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _invitationRepository.UpdateAsync(invitation);

            return Ok(new
            {
                success = true,
                message = alreadyMember ? "Zaten bu evin uyesisiniz." : "Eve basariyla katildiniz.",
                houseId = invitation.HouseId
            });
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
