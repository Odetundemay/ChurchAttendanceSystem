using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async (LoginDto dto, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(dto);
            return result.ToHttpResult();
        });

        group.MapPost("/register", async (RegisterStaffDto dto, IAuthService authService) =>
        {
            var result = await authService.RegisterStaffAsync(dto);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");
    }
}