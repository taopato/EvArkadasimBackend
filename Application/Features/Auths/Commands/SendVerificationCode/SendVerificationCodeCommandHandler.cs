using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auths.Dtos;
using Application.Services.Repositories;
using Core.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auths.Commands.SendVerificationCode
{
    public class SendVerificationCodeCommandHandler
        : IRequestHandler<SendVerificationCodeCommand, SendVerificationCodeResponseDto>
    {
        private readonly IVerificationCodeRepository _codeRepo;
        private readonly IMailService _mailService;

        public SendVerificationCodeCommandHandler(
            IVerificationCodeRepository codeRepo,
            IMailService mailService)
        {
            _codeRepo = codeRepo;
            _mailService = mailService;
        }

        public async Task<SendVerificationCodeResponseDto> Handle(
            SendVerificationCodeCommand request,
            CancellationToken cancellationToken)
        {
            var code = new Random().Next(100000, 999999).ToString();

            var entity = new VerificationCode
            {
                Email = request.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            await _codeRepo.AddAsync(entity);

            await _mailService.SendEmailAsync(
                request.Email,
                "Doğrulama Kodunuz",
                $"Kayıt kodunuz: {code}");

            return new SendVerificationCodeResponseDto();
        }
    }
}
