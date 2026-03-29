// Persistence/Services/MailService.cs
using Core.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class MailService : IMailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "info.ev.arkadasim@gmail.com";
        private const string SenderPassword = "lujs brjg mojt tuze";

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(SmtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(SenderEmail),
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
