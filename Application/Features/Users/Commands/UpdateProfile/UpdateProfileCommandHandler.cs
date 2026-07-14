using MediatR;
using Application.Features.Users.Dtos;
using Application.Services.Repositories;
using Core.Security.Hashing;
using System.Text.RegularExpressions;

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

            if (request.PhoneNumber is not null)
                user.PhoneNumber = NormalizeTurkishMobile(request.PhoneNumber);

            if (request.Iban is not null)
                user.Iban = NormalizeTurkishIban(request.Iban);

            if (request.ProfileImageUrl is not null)
                user.ProfileImageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl)
                    ? null
                    : request.ProfileImageUrl.Trim();

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
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Iban = user.Iban,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        private static string? NormalizeTurkishMobile(string value)
        {
            var digits = Regex.Replace(value ?? string.Empty, @"\D", string.Empty);
            if (digits.StartsWith("0090")) digits = digits[4..];
            else if (digits.StartsWith("90") && digits.Length > 10) digits = digits[2..];
            if (digits.StartsWith("0")) digits = digits[1..];

            if (string.IsNullOrEmpty(digits)) return null;
            if (!Regex.IsMatch(digits, @"^5\d{9}$"))
                throw new InvalidOperationException("Telefon numarası +90 5XX XXX XX XX formatında olmalıdır.");

            return $"+90{digits}";
        }

        private static string? NormalizeTurkishIban(string value)
        {
            var iban = Regex.Replace(value ?? string.Empty, @"\s", string.Empty).ToUpperInvariant();
            if (string.IsNullOrEmpty(iban) || iban == "TR") return null;
            if (!Regex.IsMatch(iban, @"^TR\d{24}$") || !HasValidIbanChecksum(iban))
                throw new InvalidOperationException("Geçerli bir Türkiye IBAN'ı girin.");

            return iban;
        }

        private static bool HasValidIbanChecksum(string iban)
        {
            var rearranged = iban[4..] + iban[..4];
            var remainder = 0;
            foreach (var character in rearranged)
            {
                var numeric = char.IsDigit(character)
                    ? character.ToString()
                    : (character - 'A' + 10).ToString();
                foreach (var digit in numeric)
                    remainder = (remainder * 10 + (digit - '0')) % 97;
            }
            return remainder == 1;
        }
    }
}
