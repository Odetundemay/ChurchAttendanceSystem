using ChurchAttendanceSystem.Api.Endpoints;
using ChurchAttendanceSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Development environment setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseMiddleware<EncryptionMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Database initialization
await DatabaseInitializer.InitializeAsync(app.Services);

// Redirect root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map endpoints
app.MapAuthEndpoints();
app.MapParentEndpoints();
app.MapChildEndpoints();
app.MapScanEndpoints();
app.MapAttendanceEndpoints();

app.Run();