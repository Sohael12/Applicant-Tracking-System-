using Stageproject_ATS_AP2025Q2.Models;

namespace Stageproject_ATS_AP2025Q2.Interfaces;

public interface IVacancyRepository
{
    Task<List<Vacancy>> GetAllAsync();
    Task<Vacancy> GetByIdAsync(int id);
    Task AddAsync(Vacancy vacancy);
    Task UpdateAsync(Vacancy vacancy);
    Task DeleteAsync(int id);
    
}
