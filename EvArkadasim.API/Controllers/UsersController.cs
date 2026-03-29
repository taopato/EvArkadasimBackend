using Application.Features.Users.Queries.GetAllUsers;
using Application.Features.Users.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result.Data);
        }

        /// <summary>
        /// PUT /api/Users/{userId}/Profile
        /// Kullanıcı profili günceller: ad soyad ve/veya şifre.
        /// </summary>
        [HttpPut("{userId:int}/Profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
        {
            var result = await _mediator.Send(new UpdateProfileCommand(
                userId,
                request.FullName,
                request.CurrentPassword,
                request.NewPassword
            ));
            return Ok(result);
        }
    }

    public record UpdateProfileRequest(
        string? FullName,
        string? CurrentPassword,
        string? NewPassword
    );
}
