using MediatR;
using Application.Features.Users.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;

namespace Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponseDto>
    {
        private readonly IUserRepository _userRepo;

        public UpdateProfileCommandHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UpdateProfileResponseDto> Handle(UpdateProfileCommand request, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

            // Ad Soyad güncelle
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                var nameParts = request.FullName.Trim().Split(' ', 2);
                user.FirstName = nameParts[0];
                user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            }

            // Şifre güncelle (mevcut şifre doğrulanmalı)
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                    throw new InvalidOperationException("Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");

                bool isCurrentValid = HashingHelper.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash);
                if (!isCurrentValid)
                    throw new InvalidOperationException("Mevcut şifreniz hatalı.");

                if (request.NewPassword.Length < 6)
                    throw new InvalidOperationException("Yeni şifre en az 6 karakter olmalıdır.");

                user.PasswordHash = HashingHelper.CreatePasswordHash(request.NewPassword);
            }

            await _userRepo.UpdateAsync(user);

            return new UpdateProfileResponseDto
            {
                IsSuccess = true,
                Message = "Profil başarıyla güncellendi.",
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email
            };
        }
    }
}
