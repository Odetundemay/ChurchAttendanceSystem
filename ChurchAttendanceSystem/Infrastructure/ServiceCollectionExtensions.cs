using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ChurchAttendanceSystem.Application;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Services;

namespace ChurchAttendanceSystem.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDb>(options => 
            options.UseSqlite(configuration.GetConnectionString("Default")));

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<IChildService, ChildService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddSingleton<IQrService, QrService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddMemoryCache();

        return services;
    }

    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = Jwt.GetValidationParameters(configuration));

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Staff"));
        });

        return services;
    }
}