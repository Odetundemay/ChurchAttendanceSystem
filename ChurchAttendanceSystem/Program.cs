using ChurchAttendanceSystem.Api.Endpoints;
using ChurchAttendanceSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

var app = builder.Build();

// Development environment setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Database initialization
await DatabaseInitializer.InitializeAsync(app.Services);

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map endpoints
app.MapAuthEndpoints();
app.MapParentEndpoints();
app.MapChildEndpoints();
app.MapScanEndpoints();
app.MapAttendanceEndpoints();

app.Run();