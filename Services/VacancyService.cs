using Stageproject_ATS_AP2025Q2.Models;
using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class VacancyService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public VacancyService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Vacancy?> GetByIdAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Vacancies.FindAsync(id);
        }

        public async Task<List<Vacancy>> GetVacanciesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Vacancies.Include(v => v.Applications).ToListAsync();
        }

        public async Task AddVacancyAsync(Vacancy vacancy)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Vacancies.Add(vacancy);
            await context.SaveChangesAsync();
        }

        public async Task<Vacancy?> GetVacancyByIdAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Vacancies.Include(v => v.Applications).FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task ApplyToVacancyAsync(Application application)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Applications.Add(application);
            await context.SaveChangesAsync();
        }

        public async Task<List<Application>> GetAllApplicationsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications.Include(a => a.Vacancy).Include(a => a.User).ToListAsync();
        }

        public async Task<bool> DeleteVacancyAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            try
            {
                var applications = context.Applications.Where(a => a.VacancyId == id);
                context.Applications.RemoveRange(applications);

                var vacancy = await context.Vacancies.FindAsync(id);
                if (vacancy != null)
                {
                    context.Vacancies.Remove(vacancy);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting vacancy: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateVacancyAsync(Vacancy vacancy)
        {
            await using var context = _contextFactory.CreateDbContext();
            var existingVacancy = await context.Vacancies.FirstOrDefaultAsync(v => v.Id == vacancy.Id);

            if (existingVacancy == null)
            {
                Console.WriteLine("Vacancy not found!");
                return;
            }

            existingVacancy.Title = vacancy.Title;
            existingVacancy.Description = vacancy.Description;
            existingVacancy.Deadline = vacancy.Deadline;
            existingVacancy.Department = vacancy.Department;
            existingVacancy.CreatedBy = vacancy.CreatedBy;

            context.Entry(existingVacancy).State = EntityState.Modified;
            await context.SaveChangesAsync();

            Console.WriteLine("Vacancy updated successfully!");
        }

        public async Task<List<Vacancy>> GetAllVacanciesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Vacancies.ToListAsync();
        }

        public async Task<List<Application>> GetApplicationsByVacancyIdAsync(int vacancyId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Applications
                .Include(a => a.User)
                .Where(a => a.VacancyId == vacancyId)
                .ToListAsync();
        }
    }
}
