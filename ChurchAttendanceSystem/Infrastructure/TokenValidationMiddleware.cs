using ChurchAttendanceSystem.Application.Interfaces;

namespace ChurchAttendanceSystem.Infrastructure;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public TokenValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        
        if (!string.IsNullOrEmpty(token))
        {
            using var scope = _serviceProvider.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            
            if (await tokenService.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has been revoked");
                return;
            }
        }

        await _next(context);
    }
}