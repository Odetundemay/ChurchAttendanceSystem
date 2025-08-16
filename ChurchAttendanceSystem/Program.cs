using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IQrService, QrService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = Jwt.GetValidationParameters(builder.Configuration));

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    o.AddPolicy("Staff", p => p.RequireRole("Admin", "Staff"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.StaffUsers.AnyAsync())
    {
        var admin = new StaffUser {
            Id = Guid.NewGuid(), FullName = "Admin", Email = "admin@example.com",
            PasswordHash = Password.Hash("Passw0rd!"), Role = "Admin"
        };
        db.StaffUsers.Add(admin);
        await db.SaveChangesAsync();
    }
}

app.MapGet("/health", () => Results.Ok(new { ok = true }));

// --- AUTH ---
app.MapPost("/api/auth/register", async (RegisterStaffDto dto, AppDb db) =>
{
    if (await db.StaffUsers.AnyAsync(x => x.Email == dto.Email)) return Results.Conflict("Email exists");
    var u = new StaffUser
    {
        Id = Guid.NewGuid(),
        FullName = dto.FullName,
        Email = dto.Email,
        PasswordHash = Password.Hash(dto.Password),
        Role = string.IsNullOrWhiteSpace(dto.Role) ? "Staff" : dto.Role
    };
    db.StaffUsers.Add(u);
    await db.SaveChangesAsync();
    return Results.Created($"/api/staff/{u.Id}", new { u.Id });
}).RequireAuthorization("AdminOnly");

app.MapPost("/api/auth/login", async (LoginDto dto, AppDb db, IConfiguration cfg) =>
{
    var u = await db.StaffUsers.FirstOrDefaultAsync(x => x.Email == dto.Email);
    if (u is null || !Password.Verify(dto.Password, u.PasswordHash)) return Results.Unauthorized();
    var token = Jwt.IssueToken(u.Id, u.Email, u.Role, cfg);
    return Results.Ok(new { token, user = new { u.Id, u.FullName, u.Email, u.Role } });
});

// --- PARENTS ---
app.MapPost("/api/parents", async (CreateParentDto dto, AppDb db) =>
{
    var parent = new Parent
    {
        Id = Guid.NewGuid(),
        FullName = dto.FullName.Trim(),
        Phone = dto.Phone,
        Email = dto.Email,
        QrSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
        CreatedAt = DateTime.UtcNow
    };
    db.Parents.Add(parent);
    await db.SaveChangesAsync();
    return Results.Created($"/api/parents/{parent.Id}", new { parent.Id });
}).RequireAuthorization("AdminOnly");

app.MapGet("/api/parents/{id:guid}/qr", async (Guid id, AppDb db, IQrService qr) =>
{
    var p = await db.Parents.FindAsync(id);
    if (p is null) return Results.NotFound();
    var payload = new { family = p.Id, s = p.QrSecret };
    var png = qr.GeneratePng(payload);
    return Results.File(png, "image/png");
}).RequireAuthorization("AdminOnly");

// --- CHILDREN ---
app.MapPost("/api/parents/{parentId:guid}/children", async (Guid parentId, CreateChildDto dto, AppDb db) =>
{
    if (!await db.Parents.AnyAsync(x => x.Id == parentId)) return Results.NotFound();
    var c = new Child { Id = Guid.NewGuid(), ParentId = parentId, FullName = dto.FullName, Group = dto.Group, IsActive = true };
    db.Children.Add(c);
    await db.SaveChangesAsync();
    return Results.Created($"/api/children/{c.Id}", new { c.Id });
}).RequireAuthorization("AdminOnly");

// --- SCAN PARENT QR (STAFF SCANS) ---
app.MapPost("/api/check/scan", async (ScanDto dto, AppDb db) =>
{
    var parent = await db.Parents.Include(p => p.Children.Where(k => k.IsActive))
        .FirstOrDefaultAsync(p => p.Id == dto.Family && p.QrSecret == dto.S);
    if (parent is null) return Results.BadRequest(new { error = "Invalid QR" });

    var kids = parent.Children.Select(k => new { k.Id, k.FullName, k.Group }).ToList();
    return Results.Ok(new { parent = new { parent.Id, parent.FullName }, children = kids });
}).RequireAuthorization("Staff");

// --- MARK ATTENDANCE ---
app.MapPost("/api/attendance/mark", async (MarkAttendanceDto dto, AppDb db, ClaimsPrincipal user) =>
{
    if (dto.Action is not ("CheckIn" or "CheckOut")) return Results.BadRequest("Invalid action");
    var staffIdStr = user.FindFirstValue("uid");
    if (staffIdStr is null) return Results.Unauthorized();
    var staffId = Guid.Parse(staffIdStr);

    var now = DateTime.UtcNow;
    var session = now.ToString("yyyy-MM-dd");

    var entries = dto.ChildIds.Select(id => new AttendanceLog
    {
        Id = Guid.NewGuid(),
        ChildId = id,
        Action = dto.Action,
        TimestampUtc = now,
        HandledByUserId = staffId,
        SessionDate = session
    });

    await db.AttendanceLogs.AddRangeAsync(entries);
    await db.SaveChangesAsync();
    return Results.Ok(new { ok = true, count = dto.ChildIds.Count });
}).RequireAuthorization("Staff");

// --- REPORTS ---
app.MapGet("/api/reports/session/{date}", async (string date, AppDb db) =>
{
    var logs = await db.AttendanceLogs
        .Where(x => x.SessionDate == date)
        .Include(x => x.Child).ThenInclude(c => c.Parent)
        .OrderBy(x => x.TimestampUtc)
        .Select(x => new {
            x.Action,
            x.TimestampUtc,
            Child = x.Child.FullName,
            Parent = x.Child.Parent.FullName,
            Group = x.Child.Group
        }).ToListAsync();

    return Results.Ok(logs);
}).RequireAuthorization("Staff");

app.Run();