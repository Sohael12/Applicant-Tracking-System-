using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Models;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class ApplicationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ApplicationService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Application>> GetAllApplicationsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<Application?> GetByIdAsync(int applicationId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task SaveApplicationAsync(Application application)
        {
            await using var context = _contextFactory.CreateDbContext();
            if (application.Id == 0)
            {
                context.Applications.Add(application);
            }
            else
            {
                context.Applications.Update(application);
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<Application>> GetAllAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .Include(a => a.StatusHistories)
                .ToListAsync();
        }

        public async Task<List<Application>> GetApplicationsByVacancyIdAsync(int vacancyId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.User)
                .Where(a => a.VacancyId == vacancyId)
                .ToListAsync();
        }

        public async Task<Application> GetApplicationByIdAsync(int applicationId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var application = await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {applicationId} not found.");
            }

            return application;
        }

        public async Task<Application?> GetByVacancyAndUserAsync(int vacancyId, string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.VacancyId == vacancyId && a.UserId == userId);
        }

        public async Task<List<Application>> GetByUserIdAsync(string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.Vacancy)
                .Include(a => a.StatusHistories)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

      // ✅ Get all unique users who have applied and are eligible for notifications
        public async Task<List<AppUser>> GetAllApplicantsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            // Load users with applications and status histories
            var users = await context.Users
                .Include(u => u.Applications)
                    .ThenInclude(a => a.StatusHistories)
                .Where(u => u.Applications != null && u.Applications.Any())
                .ToListAsync();

            // Filter out users whose **latest application status** is Accepted, Rejected, or TrainingPhase
            var eligibleUsers = users
                .Where(user =>
                    !user.Applications.Any(app =>
                        app.StatusHistories != null &&
                        app.StatusHistories
                            .OrderByDescending(sh => sh.Date)
                            .FirstOrDefault() is StatusHistory latestStatus &&
                        (latestStatus.Status == StatusHistory.ApplicationStatus.Accepted ||
                         latestStatus.Status == StatusHistory.ApplicationStatus.Rejected ||
                         latestStatus.Status == StatusHistory.ApplicationStatus.TrainingPhase)
                    )
                )
                .ToList();

            return eligibleUsers;
        }
    }
}