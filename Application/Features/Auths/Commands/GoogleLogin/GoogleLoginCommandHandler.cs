using Application.Features.Auths.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;
using Core.Security.JWT;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auths.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler
        : IRequestHandler<GoogleLoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenHelper _tokenHelper;

        public GoogleLoginCommandHandler(
            IUserRepository userRepo,
            ITokenHelper tokenHelper)
        {
            _userRepo = userRepo;
            _tokenHelper = tokenHelper;
        }

        public async Task<LoginResponseDto> Handle(
            GoogleLoginCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null)
            {
                var safeFullName = string.IsNullOrWhiteSpace(request.FullName)
                    ? request.Email
                    : request.FullName.Trim();

                var names = safeFullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var firstName = names.Length > 0 ? names[0] : request.Email;
                var lastName = names.Length > 1 ? names[1] : string.Empty;

                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = request.Email,
                    PasswordHash = HashingHelper.CreatePasswordHash(Guid.NewGuid().ToString("N")),
                    CreatedAt = DateTime.UtcNow,
                    RegistrationDate = DateTime.UtcNow
                };

                await _userRepo.AddAsync(user);
            }

            var accessToken = _tokenHelper.CreateToken(user);

            return new LoginResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Token = accessToken.Token,
                Message = "Giriş başarılı!"
            };
        }
    }
}

