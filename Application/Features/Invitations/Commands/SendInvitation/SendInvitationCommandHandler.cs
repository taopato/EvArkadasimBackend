using Application.Features.Invitations.Dtos;
using Application.Services.Repositories;
using Core.Interfaces;
using Core.Utilities.Results;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Invitations.Commands.SendInvitation
{
    public class SendInvitationCommandHandler : IRequestHandler<SendInvitationCommand, Response<SendInvitationResponseDto>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMailService _mailService;
        private readonly string _webBaseUrl;

        public SendInvitationCommandHandler(
            IInvitationRepository invitationRepository,
            IMailService mailService,
            IConfiguration configuration)
        {
            _invitationRepository = invitationRepository;
            _mailService = mailService;
            _webBaseUrl = (configuration["AppUrls:WebBaseUrl"] ?? "https://www.evarkadasim.co").TrimEnd('/');
        }

        public async Task<Response<SendInvitationResponseDto>> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
        {
            var token = Guid.NewGuid().ToString("N")[..16].ToUpper();
            var expiresAt = DateTime.UtcNow.AddDays(7);

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

            var inviteLink = $"{_webBaseUrl}/davet-kabul?token={token}&houseId={request.HouseId}&email={Uri.EscapeDataString(request.Email)}";

            var subject = "EvArkadasim - Eve Davet Edildiniz!";
            var body = $@"
<div style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 10px;"">
    <h2 style=""color: #4CAF50;"">Merhaba,</h2>
    <p>Sizi ev grubuna katılmaya davet ettiler!</p>
    <p>Aşağıdaki butona tıklayarak hızlıca kayıt olun ve direkt eve dahil olun:</p>
    <div style=""text-align: center; margin: 30px 0;"">
        <a href=""{inviteLink}"" style=""background-color: #4CAF50; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;"">
            Daveti Kabul Et
        </a>
    </div>
    <p style=""font-size: 0.85em; color: #666;"">
        Eğer yukarıdaki buton çalışmıyorsa, şu linki tarayıcınıza yapıştırabilirsiniz:<br>
        <a href=""{inviteLink}"" style=""color: #2196F3; word-break: break-all;"">{inviteLink}</a>
    </p>
    <p>Link <b>{expiresAt:dd.MM.yyyy}</b> tarihine kadar geçerlidir.</p>
    <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"">
    <p style=""font-size: 0.9em;"">EvArkadasim uygulaması ile ev arkadaşlarınızla harcamalarınızı kolayca takip edin.</p>
    <p>Saygılarımızla,<br><strong>EvArkadasim Ekibi</strong></p>
</div>";

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
