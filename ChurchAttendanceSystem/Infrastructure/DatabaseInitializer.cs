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
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        await db.Database.EnsureCreatedAsync();
        
        if (!await db.StaffUsers.AnyAsync())
        {
            var adminEmail = config["DefaultAdmin:Email"] ?? "admin@church.com";
            var adminPassword = config["DefaultAdmin:Password"] ?? Guid.NewGuid().ToString()[..8];
            
            var admin = new StaffUser
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                PasswordHash = Password.Hash(adminPassword),
                Role = "Admin"
            };
            
            db.StaffUsers.Add(admin);
            await db.SaveChangesAsync();
            
            Console.WriteLine($"Default admin created: {adminEmail} / {adminPassword}");
        }
    }
}