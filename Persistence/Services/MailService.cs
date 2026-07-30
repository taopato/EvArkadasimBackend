// Persistence/Services/MailService.cs
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class MailService : IMailService
    {
        private const string DefaultSmtpServer = "smtp.gmail.com";
        private const int DefaultSmtpPort = 587;

        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _smtpUsername;
        private readonly string _senderPassword;
        private readonly string _displayName;
        private readonly string? _replyTo;

        public MailService(IConfiguration configuration)
        {
            _smtpServer = ResolveSetting(configuration["SmtpSettings:Server"], DefaultSmtpServer);
            _smtpPort = int.TryParse(ResolveSetting(configuration["SmtpSettings:Port"], DefaultSmtpPort.ToString()), out var configuredPort)
                ? configuredPort
                : DefaultSmtpPort;
            _senderEmail = ResolveSetting(configuration["SmtpSettings:SenderEmail"], string.Empty);
            _smtpUsername = ResolveSetting(configuration["SmtpSettings:Username"], _senderEmail);
            _senderPassword = ResolveSetting(configuration["SmtpSettings:Password"], string.Empty);
            _displayName = ResolveSetting(configuration["SmtpSettings:DisplayName"], "Roomora");
            _replyTo = ResolveSetting(configuration["SmtpSettings:ReplyTo"], string.Empty);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var senderEmail = RequireSetting(_senderEmail, "SmtpSettings:SenderEmail");
            var senderPassword = RequireSetting(_senderPassword, "SmtpSettings:Password");
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(RequireSetting(_smtpUsername, "SmtpSettings:Username"), senderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, _displayName, Encoding.UTF8),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };
            if (!string.IsNullOrWhiteSpace(_replyTo))
                message.ReplyToList.Add(new MailAddress(_replyTo));
            message.To.Add(to);

            // Asenkron gönderim
            await client.SendMailAsync(message);
        }

        private static string ResolveSetting(string? configuredValue, string fallbackValue)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return fallbackValue;

            var value = configuredValue.Trim();
            return value.StartsWith("CHANGE_ME_", System.StringComparison.OrdinalIgnoreCase)
                ? fallbackValue
                : value;
        }

        private static string RequireSetting(string? configuredValue, string settingName)
        {
            if (string.IsNullOrWhiteSpace(configuredValue) ||
                configuredValue.Trim().StartsWith("CHANGE_ME_", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{settingName} yapılandırılmalıdır. Gizli değerleri kaynak kodda tutmayın; environment variable veya secret store kullanın.");
            }

            return configuredValue.Trim();
        }
    }
}
