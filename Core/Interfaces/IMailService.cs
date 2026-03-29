using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
