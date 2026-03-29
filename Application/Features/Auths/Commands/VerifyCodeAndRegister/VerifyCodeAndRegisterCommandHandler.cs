// Application/Features/Auths/Commands/VerifyCodeAndRegister/VerifyCodeAndRegisterCommandHandler.cs
using MediatR;
using Application.Features.Auths.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;
using Core.Security.JWT;
using Domain.Entities;

namespace Application.Features.Auths.Commands.VerifyCodeAndRegister
{
    public class VerifyCodeAndRegisterCommandHandler
        : IRequestHandler<VerifyCodeAndRegisterCommand, VerifyCodeAndRegisterResponseDto>
    {
        private readonly IVerificationCodeRepository _codeRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITokenHelper _tokenHelper;

        public VerifyCodeAndRegisterCommandHandler(
            IVerificationCodeRepository codeRepo,
            IUserRepository userRepo,
            ITokenHelper tokenHelper)
        {
            _codeRepo = codeRepo;
            _userRepo = userRepo;
            _tokenHelper = tokenHelper;
        }

        public async Task<VerifyCodeAndRegisterResponseDto> Handle(
            VerifyCodeAndRegisterCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Kod doğrulama
            var codeEntity = await _codeRepo
                .GetByEmailAndCodeAsync(request.Email, request.Code)
                ?? throw new InvalidOperationException("Geçersiz kod veya e-posta.");
            if (codeEntity.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Kodun süresi dolmuştur.");

            // 2) Eğer FullName boşsa — sadece kod doğrulama
            if (string.IsNullOrEmpty(request.FullName))
            {
                return new VerifyCodeAndRegisterResponseDto
                {
                    IsSuccess = true,
                    Message = "Kod doğrulandı."
                };
            }

            // 3) Kayıt akışı
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new InvalidOperationException("Bu e-posta zaten kayıtlı.");

            var names = request.FullName.Split(' ', 2);
            var user = new User
            {
                FirstName = names[0],
                LastName = names.Length > 1 ? names[1] : "",
                Email = request.Email,
                PasswordHash = HashingHelper.CreatePasswordHash(request.Password),
                CreatedAt = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow
            };
            await _userRepo.AddAsync(user);
            await _codeRepo.DeleteAsync(codeEntity);
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            string? token = null;
            try
            {
                var accessToken = _tokenHelper.CreateToken(user);
                token = accessToken.Token;
            }
            catch
            {
                token = null;
            }

            return new VerifyCodeAndRegisterResponseDto
            {
                IsSuccess = true,
                Message = "Kullanici basariyla kaydedildi.",
                Id = user.Id,
                Email = user.Email,
                FullName = fullName,
                Token = token
            };
        }
    }
}
