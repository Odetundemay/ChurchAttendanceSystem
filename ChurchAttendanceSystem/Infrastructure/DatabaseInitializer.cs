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
        
        var adminEmail = config["DefaultAdmin:Email"] ?? "admin@church.com";
        var adminPassword = config["DefaultAdmin:Password"] ?? "SecurePass123!";
        
        var existingAdmin = await db.StaffUsers.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingAdmin == null)
        {
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
        else if (existingAdmin.Role != "Admin")
        {
            existingAdmin.Role = "Admin";
            existingAdmin.PasswordHash = Password.Hash(adminPassword);
            await db.SaveChangesAsync();
            
            Console.WriteLine($"Admin user updated: {adminEmail} / {adminPassword}");
        }
        
        // Add test parent if none exist
        if (!await db.Parents.AnyAsync())
        {
            var testParent = new Parent
            {
                Id = Guid.Parse("b2a4fe8b-a30c-4f00-8c99-b0fd75c6a106"),
                FirstName = "John",
                LastName = "Smith",
                Gender = "Male",
                Phone = "555-0123",
                Email = "john.smith@example.com",
                QrSecret = "dGVzdFNlY3JldA=="
            };
            
            var testChild = new Child
            {
                Id = Guid.NewGuid(),
                ParentId = testParent.Id,
                FirstName = "Emma",
                LastName = "Smith",
                Gender = "Female",
                DateOfBirth = "2018-05-15"
            };
            
            db.Parents.Add(testParent);
            db.Children.Add(testChild);
            await db.SaveChangesAsync();
            
            Console.WriteLine("Test parent and child created for QR testing");
        }
    }
}