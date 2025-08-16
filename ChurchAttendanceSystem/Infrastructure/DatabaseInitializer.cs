using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application;
using ChurchAttendanceSystem.Domain;

namespace ChurchAttendanceSystem.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        
        await db.Database.EnsureCreatedAsync();
        
        if (!await db.StaffUsers.AnyAsync())
        {
            var admin = new StaffUser
            {
                Id = Guid.NewGuid(),
                FullName = "Admin",
                Email = "admin@example.com",
                PasswordHash = Password.Hash("Passw0rd!"),
                Role = "Admin"
            };
            
            db.StaffUsers.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}