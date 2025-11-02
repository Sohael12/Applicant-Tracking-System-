using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Interfaces;
using Stageproject_ATS_AP2025Q2.Models;

namespace Stageproject_ATS_AP2025Q2.Repositories;

public class VacancyRepository : IVacancyRepository
{
    private readonly AppDbContext _context;

    public VacancyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vacancy>> GetAllAsync()
    {
        return await _context.Vacancies.ToListAsync();
    }

    public async Task<Vacancy> GetByIdAsync(int id)
    {
        return await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(Vacancy vacancy)
    {
        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        _context.Vacancies.Update(vacancy);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var vacancy = await _context.Vacancies.FindAsync(id);
        if (vacancy != null)
        {
            _context.Vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();
        }
    }

    public Task<List<Vacancy>> GetByApplicantEmailAsync(string email)
    {
        throw new NotImplementedException();
    }
    
    
}
