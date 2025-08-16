using System.Security.Claims;
using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class AttendanceEndpoints
{
    public static void MapAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attendance").WithTags("Attendance");

        group.MapPost("/mark", async (MarkAttendanceDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.MarkAttendanceAsync(dto, staffId);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapGet("/reports/session/{date}", async (string date, IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetSessionReportAsync(date);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");
    }
}