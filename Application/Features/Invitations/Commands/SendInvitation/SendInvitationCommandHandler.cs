using Application.Features.Invitations.Dtos;
using Application.Services.Repositories;
using Core.Utilities.Results;
using Domain.Entities;
using MediatR;
using Core.Interfaces; // Eğer IMailService burada tanımlıysa

namespace Application.Features.Invitations.Commands.SendInvitation
{
    public class SendInvitationCommandHandler : IRequestHandler<SendInvitationCommand, Response<SendInvitationResponseDto>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMailService _mailService;

        public SendInvitationCommandHandler(
            IInvitationRepository invitationRepository,
            IMailService mailService)
        {
            _invitationRepository = invitationRepository;
            _mailService = mailService;
        }

        public async Task<Response<SendInvitationResponseDto>> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddHours(2);

            var invitation = new Invitation
            {
                Email = request.Email,
                HouseId = request.HouseId,
                Token = code,
                SentAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Status = "Pending"
            };

            await _invitationRepository.AddAsync(invitation);

            // 📧 Mail gönderimi
            string subject = "EvArkadasim - Davet Kodunuz";
            string body = $"Merhaba,\n\nEv davet kodunuz: {code}\nKodun geçerlilik süresi: {expiresAt:dd.MM.yyyy HH:mm}\n\nEvArkadasim uygulaması ile daveti kabul edebilirsiniz.";

            await _mailService.SendEmailAsync(request.Email, subject, body);

            var response = new SendInvitationResponseDto
            {
                InvitationCode = code,
                ExpiresAt = expiresAt
            };

            return new Response<SendInvitationResponseDto>(response, true, "Davet başarıyla gönderildi.");
        }
    }
}
