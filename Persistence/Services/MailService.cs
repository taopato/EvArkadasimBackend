// Persistence/Services/MailService.cs
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class MailService : IMailService
    {
        private const string DefaultSmtpServer = "smtp.gmail.com";
        private const int DefaultSmtpPort = 587;
        private const string DefaultSenderEmail = "info.ev.arkadasim@gmail.com";
        private const string DefaultSenderPassword = "lujs brjg mojt tuze";

        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public MailService(IConfiguration configuration)
        {
            _smtpServer = configuration["SmtpSettings:Server"] ?? DefaultSmtpServer;
            _smtpPort = int.TryParse(configuration["SmtpSettings:Port"], out var configuredPort)
                ? configuredPort
                : DefaultSmtpPort;
            _senderEmail = configuration["SmtpSettings:SenderEmail"] ?? DefaultSenderEmail;
            _senderPassword = configuration["SmtpSettings:Password"] ?? DefaultSenderPassword;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            // Asenkron gönderim
            await client.SendMailAsync(message);
        }
    }
}
