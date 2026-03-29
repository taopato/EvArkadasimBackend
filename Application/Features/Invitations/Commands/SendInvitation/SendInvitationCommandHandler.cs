using Application.Features.Invitations.Dtos;
using Application.Services.Repositories;
using Core.Utilities.Results;
using Domain.Entities;
using MediatR;
using Core.Interfaces;

namespace Application.Features.Invitations.Commands.SendInvitation
{
    public class SendInvitationCommandHandler : IRequestHandler<SendInvitationCommand, Response<SendInvitationResponseDto>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMailService _mailService;
        // Web uygulamasının base URL'i - production'da appsettings'ten okunabilir
        private const string WebBaseUrl = "https://evarkadasim.com";

        public SendInvitationCommandHandler(
            IInvitationRepository invitationRepository,
            IMailService mailService)
        {
            _invitationRepository = invitationRepository;
            _mailService = mailService;
        }

        public async Task<Response<SendInvitationResponseDto>> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
        {
            // Güvenli, tahmin edilemez bir token üret (6 haneli koddan daha güvenli)
            var token = Guid.NewGuid().ToString("N")[..16].ToUpper();
            var expiresAt = DateTime.UtcNow.AddDays(7); // 7 gün geçerli

            var invitation = new Invitation
            {
                Email = request.Email,
                HouseId = request.HouseId,
                Token = token,
                SentAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Status = "Pending"
            };

            await _invitationRepository.AddAsync(invitation);

            // Davet linki oluştur
            var inviteLink = $"{WebBaseUrl}/davet-kabul?token={token}&houseId={request.HouseId}&email={Uri.EscapeDataString(request.Email)}";

            // HTML e-posta gönder
            string subject = "EvArkadasim - Eve Davet Edildiniz!";
            string body = $@"
Merhaba,

Sizi ev grubuna katılmaya davet ettiler!

Aşağıdaki butona tıklayarak hızlıca kayıt olun ve direkt eve dahil olun:

👉 {inviteLink}

Link {expiresAt:dd.MM.yyyy} tarihine kadar geçerlidir.

EvArkadasim uygulaması ile ev arkadaşlarınızla harcamalarınızı kolayca takip edin.

Saygılarımızla,
EvArkadasim Ekibi
";

            await _mailService.SendEmailAsync(request.Email, subject, body);

            var response = new SendInvitationResponseDto
            {
                InvitationCode = token,
                ExpiresAt = expiresAt
            };

            return new Response<SendInvitationResponseDto>(response, true, "Davet başarıyla gönderildi.");
        }
    }
}
