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
else
{
    // Enable Swagger in production for debugging
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add request logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path} from {RemoteIp}", 
        context.Request.Method, 
        context.Request.Path, 
        context.Connection.RemoteIpAddress);
    
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        logger.LogInformation("Has Authorization header");
    }
    else
    {
        logger.LogWarning("No Authorization header");
    }
    
    await next();
});

app.UseCors("AllowFrontend");
app.UseMiddleware<EncryptionMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Database initialization
try
{
    await DatabaseInitializer.InitializeAsync(app.Services);
    app.Logger.LogInformation("Database initialized successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialization failed");
    throw;
}

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