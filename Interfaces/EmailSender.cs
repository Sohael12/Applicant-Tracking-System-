using System.Threading.Tasks;

namespace Stageproject_ATS_AP2025Q2.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
