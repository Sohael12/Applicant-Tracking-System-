using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;

public class UserRoleService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public UserRoleService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> IsAdminAsync(string userId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.UserRoles.AnyAsync(r => r.UserId == userId && r.Role == "Admin");
    }

    // Add this method for checking Manager role
    public async Task<bool> IsManagerAsync(string userId)
    {
        await using var context = _contextFactory.CreateDbContext();
        // Match exactly what’s in the database
        return await context.UserRoles.AnyAsync(r => r.UserId == userId && r.Role == "Management");
    }
}
