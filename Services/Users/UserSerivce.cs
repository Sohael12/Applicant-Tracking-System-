using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Stageproject_ATS_AP2025Q2.Data;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all users who have submitted at least one job application.
    /// </summary>
    /// <returns>A list of AppUsers with job applications.</returns>

    public async Task<List<AppUser>> GetUsersWithApplicationsAsync()
    {
        return await _context.Users
            .Include(u => u.Applications)
            .Where(u => u.Applications.Any())
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }
    public async Task<AppUser?> GetUserByEmailAsync(string email)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email);
}
public async Task<bool> IsManagerAsync(string userId)
{
    // Check if the user has a UserRole with Role = "Management"
    return await _context.UserRoles
        .AnyAsync(ur => ur.UserId == userId && ur.Role == "Management");
}

    public async Task<bool> IsAdminAsync(string userId)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role == "Admin");
    }
    
   public async Task<List<AppUser>> GetAllAdminsAndManagersAsync()
{
    // Haal alle gebruikers die een rol Admin of Management hebben
    return await _context.Users
        .Where(u => _context.UserRoles
            .Any(ur => ur.UserId == u.Id && (ur.Role == "Admin" || ur.Role == "Management")))
        .OrderBy(u => u.FirstName)
        .ThenBy(u => u.LastName)
        .ToListAsync();
}


    
}
