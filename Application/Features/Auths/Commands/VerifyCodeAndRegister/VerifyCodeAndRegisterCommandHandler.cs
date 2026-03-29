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
        private readonly IInvitationRepository _invitationRepository;
        private readonly IHouseRepository _houseRepository;

        public VerifyCodeAndRegisterCommandHandler(
            IVerificationCodeRepository codeRepo,
            IUserRepository userRepo,
            ITokenHelper tokenHelper,
            IInvitationRepository invitationRepository,
            IHouseRepository houseRepository)
        {
            _codeRepo = codeRepo;
            _userRepo = userRepo;
            _tokenHelper = tokenHelper;
            _invitationRepository = invitationRepository;
            _houseRepository = houseRepository;
        }

        public async Task<VerifyCodeAndRegisterResponseDto> Handle(
            VerifyCodeAndRegisterCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Davet tokenı ile gelen kayıt mı? (InvitationToken varsa doğrudan kayıt, kod gerekmez)
            Invitation? invitation = null;
            bool isInvitationFlow = !string.IsNullOrEmpty(request.InvitationToken);

            if (isInvitationFlow)
            {
                // Davet tokenını doğrula
                invitation = await _invitationRepository.GetByCodeAsync(request.InvitationToken!);
                if (invitation == null)
                    throw new InvalidOperationException("Geçersiz davet linki.");
                if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow)
                    throw new InvalidOperationException("Davet linkinin süresi dolmuştur.");
                if (invitation.Status != "Pending")
                    throw new InvalidOperationException("Bu davet linki daha önce kullanılmış.");
                // E-posta kontrolü: link hangi e-posta ile gönderildiyse o kullanılmalı
                if (!string.IsNullOrEmpty(invitation.Email) &&
                    !invitation.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Bu davet linki farklı bir e-posta adresine gönderilmiştir.");
            }
            else
            {
                // 2) Normal kayıt akışı: Doğrulama kodu kontrolü
                var codeEntity = await _codeRepo
                    .GetByEmailAndCodeAsync(request.Email, request.Code)
                    ?? throw new InvalidOperationException("Geçersiz kod veya e-posta.");
                if (codeEntity.ExpiresAt < DateTime.UtcNow)
                    throw new InvalidOperationException("Kodun süresi dolmuştur.");

                // Eğer FullName boşsa — sadece kod doğrulama
                if (string.IsNullOrEmpty(request.FullName))
                {
                    return new VerifyCodeAndRegisterResponseDto
                    {
                        IsSuccess = true,
                        Message = "Kod doğrulandı."
                    };
                }

                await _codeRepo.DeleteAsync(codeEntity);
            }

            // 3) Kayıt akışı (her iki akış için ortak)
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new InvalidOperationException("Bu e-posta zaten kayıtlı.");

            var names = (request.FullName ?? "").Split(' ', 2);
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

            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            // 4) Davet akışı ise: kullanıcıyı eve ekle
            int? joinedHouseId = null;
            if (isInvitationFlow && invitation != null)
            {
                try
                {
                    var house = await _houseRepository.GetByIdAsync(invitation.HouseId);
                    if (house != null)
                    {
                        // Kullanıcıyı eve ekle (AddMemberAsync kullan)
                        await _houseRepository.AddMemberAsync(new HouseMember
                        {
                            HouseId = house.Id,
                            UserId = user.Id,
                            JoinedDate = DateTime.UtcNow
                        });
                        joinedHouseId = house.Id;
                    }

                    // Daveti kabul edildi olarak işaretle
                    invitation.Status = "Accepted";
                    invitation.AcceptedByUserId = user.Id;
                    invitation.AcceptedAt = DateTime.UtcNow;
                    await _invitationRepository.UpdateAsync(invitation);
                }
                catch
                {
                    // Eve ekleme başarısız olsa bile kayıt tamamlanmış sayılır
                }
            }

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
                Message = joinedHouseId.HasValue
                    ? $"Kullanıcı başarıyla kaydedildi ve eve eklendi."
                    : "Kullanici basariyla kaydedildi.",
                Id = user.Id,
                Email = user.Email,
                FullName = fullName,
                Token = token,
                JoinedHouseId = joinedHouseId
            };
        }
    }
}
