using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Models;
using Microsoft.EntityFrameworkCore;

namespace Stageproject_ATS_AP2025Q2.Services
{
    public class StatusHistoryService
    {
        private readonly AppDbContext _context;

        public StatusHistoryService(AppDbContext context)
        {
            _context = context;
        }

        // Haalt statusgeschiedenis op voor een specifieke applicationId
        public async Task<List<StatusHistory>> GetStatusHistoryByApplicationIdAsync(int applicationId) =>
            await _context.StatusHistories
                .Where(sh => sh.ApplicationId == applicationId)
                .OrderByDescending(sh => sh.Date)
                .ToListAsync();

        // Haalt alle statusgeschiedenissen op (voor admin e.d.)
        public async Task<List<StatusHistory>> GetAllStatusHistoryAsync() =>
            await _context.StatusHistories.ToListAsync();

        public async Task SaveStatusHistoryAsync(StatusHistory statusHistory)
        {
            _context.StatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusHistoryAsync(StatusHistory statusHistory)
        {
            var existing = await _context.StatusHistories.FindAsync(statusHistory.Id);
            if (existing is null) return;

            existing.Status = statusHistory.Status;
            existing.Comment = statusHistory.Comment;
            existing.Date = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteStatusHistoryAsync(int id)
        {
            var status = await _context.StatusHistories.FindAsync(id);
            if (status is null) return;

            _context.StatusHistories.Remove(status);
            await _context.SaveChangesAsync();
        }
    }
}
