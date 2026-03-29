// Application/Features/Auths/Commands/ResetPassword/ResetPasswordCommandHandler.cs
using MediatR;
using Application.Features.Auths.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;
using Domain.Entities;

namespace Application.Features.Auths.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler
        : IRequestHandler<ResetPasswordCommand, ResetPasswordResponseDto>
    {
        private readonly IVerificationCodeRepository _codeRepo;
        private readonly IUserRepository _userRepo;

        public ResetPasswordCommandHandler(
            IVerificationCodeRepository codeRepo,
            IUserRepository userRepo)
        {
            _codeRepo = codeRepo;
            _userRepo = userRepo;
        }

        public async Task<ResetPasswordResponseDto> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Kod doğrulama
            var codeEntity = await _codeRepo
                .GetByEmailAndCodeAsync(request.Email, request.Code)
                ?? throw new InvalidOperationException("Geçersiz kod veya e-posta.");
            if (codeEntity.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Kodun süresi dolmuştur.");

            // 2) Kullanıcıyı bul
            var user = await _userRepo.GetByEmailAsync(request.Email)
                       ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

            // 3) Şifreyi güncelle
            user.PasswordHash = HashingHelper.CreatePasswordHash(request.NewPassword);
            await _userRepo.UpdateAsync(user);
            await _codeRepo.DeleteAsync(codeEntity);

            return new ResetPasswordResponseDto
            {
                IsSuccess = true,
                Message = "Şifre başarıyla sıfırlandı."
            };
        }
    }
}
