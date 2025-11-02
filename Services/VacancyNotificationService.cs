using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Models;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class VacancyNotificationService
    {
        private readonly ApplicationService _applicationService;
        private readonly GraphEmailService _graphEmailService;
        private readonly AppDbContext _context;

        public VacancyNotificationService(ApplicationService applicationService, GraphEmailService graphEmailService, AppDbContext context)
        {
            _applicationService = applicationService;
            _graphEmailService = graphEmailService;
            _context = context;
        }

        public async Task NotifyUsersAboutNewVacancy(Vacancy vacancy)
        {
            // ✅ Haal template op uit DB
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Name == "NewVacancyEmail");
            if (template == null)
                throw new Exception("Email template 'NewVacancyEmail' not found in database.");

            // ✅ Haal sollicitanten op
            var allApplicants = await _applicationService.GetAllApplicantsAsync();

            foreach (var user in allApplicants)
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    continue;

                string subject = $"New Job Opening: {vacancy.Title}";

                // ✅ Vervang placeholders
                string body = template.HtmlContent
                    .Replace("{FirstName}", user.FirstName ?? "")
                    .Replace("{LastName}", user.LastName ?? "")
                    .Replace("{VacancyTitle}", vacancy.Title ?? "")
                    .Replace("{Department}", vacancy.Department ?? "")
.Replace("{Deadline}", vacancy.Deadline?.ToString("dd MMM yyyy") ?? "N/A")
                    .Replace("{Year}", DateTime.Now.Year.ToString());

                await _graphEmailService.SendEmailAsync(user.Email, subject, body);
            }
        }
    }
}
