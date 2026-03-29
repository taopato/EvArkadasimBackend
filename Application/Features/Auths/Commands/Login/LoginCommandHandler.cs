using Application.Features.Auths.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;
using Core.Security.JWT;
using MediatR;

namespace Application.Features.Auths.Commands.Login
{
    public class LoginCommandHandler
        : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenHelper _tokenHelper;

        public LoginCommandHandler(
            IUserRepository userRepo,
            ITokenHelper tokenHelper)
        {
            _userRepo = userRepo;
            _tokenHelper = tokenHelper;
        }

        public async Task<LoginResponseDto> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email)
                       ?? throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

            if (!HashingHelper.VerifyPasswordHash(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");


            var accessToken = _tokenHelper.CreateToken(user);

            return new LoginResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Token = accessToken.Token,
                Message = "Giriş başarılı!"
            };
        }
    }
}
