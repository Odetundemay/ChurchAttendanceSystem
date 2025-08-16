using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Domain;

namespace ChurchAttendanceSystem.Infrastructure;

public static class DatabaseMigration
{
    public static async Task MigrateToNewStructureAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        
        try
        {
            // Check if migration is needed by checking if FirstName column exists
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Parents') WHERE name = 'FirstName'";
            var hasNewStructure = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
            
            if (hasNewStructure) return; // Already migrated
            
            // Create backup tables
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE Parents_New (
                    Id TEXT PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    QrSecret TEXT NOT NULL,
                    CreatedAt TEXT
                )");
            
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE Children_New (
                    Id TEXT PRIMARY KEY,
                    ParentId TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    DateOfBirth TEXT DEFAULT '2020-01-01',
                    Allergies TEXT,
                    EmergencyContact TEXT,
                    MedicalNotes TEXT,
                    PhotoUrl TEXT DEFAULT '',
                    IsActive INTEGER DEFAULT 1
                )");
            
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE StaffUsers_New (
                    Id TEXT PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT DEFAULT 'Staff',
                    CreatedAt TEXT
                )");
            
            // Migrate data
            await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Parents_New (Id, FirstName, LastName, Phone, Email, QrSecret, CreatedAt)
                SELECT Id, 
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, 1, instr(FullName, ' ') - 1) ELSE FullName END,
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, instr(FullName, ' ') + 1) ELSE '' END,
                       Phone, Email, QrSecret, CreatedAt
                FROM Parents");
            
            await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Children_New (Id, ParentId, FirstName, LastName, DateOfBirth, IsActive)
                SELECT Id, ParentId,
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, 1, instr(FullName, ' ') - 1) ELSE FullName END,
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, instr(FullName, ' ') + 1) ELSE '' END,
                       '2020-01-01', IsActive
                FROM Children");
            
            await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO StaffUsers_New (Id, FirstName, LastName, Email, PasswordHash, Role, CreatedAt)
                SELECT Id,
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, 1, instr(FullName, ' ') - 1) ELSE FullName END,
                       CASE WHEN instr(FullName, ' ') > 0 THEN substr(FullName, instr(FullName, ' ') + 1) ELSE '' END,
                       Email, PasswordHash, Role, CreatedAt
                FROM StaffUsers");
            
            // Drop old tables and rename new ones
            await db.Database.ExecuteSqlRawAsync("DROP TABLE Parents");
            await db.Database.ExecuteSqlRawAsync("DROP TABLE Children");
            await db.Database.ExecuteSqlRawAsync("DROP TABLE StaffUsers");
            
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Parents_New RENAME TO Parents");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Children_New RENAME TO Children");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE StaffUsers_New RENAME TO StaffUsers");
            
            Console.WriteLine("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration failed: {ex.Message}");
            // Continue with normal initialization
        }
    }
}