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

        group.MapPost("/checkin", async (CheckInDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.CheckInAsync(dto, staffId);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/checkout", async (CheckOutDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.CheckOutAsync(dto, staffId);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/list", async (IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetAttendanceRecordsAsync();
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/mark", async (MarkAttendanceDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.MarkAttendanceAsync(dto, staffId);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/reports/session", async (SessionReportDto dto, IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetSessionReportAsync(dto.Date);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapGet("/today", async (IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetTodaysAttendanceAsync();
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapGet("/recent", async (IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetRecentActivityAsync();
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapGet("/parent/{parentId:guid}/checked-in", async (Guid parentId, IAttendanceService attendanceService) =>
        {
            var result = await attendanceService.GetCheckedInChildrenByParentAsync(parentId);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/checkout-by-parent", async (CheckOutByParentDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.CheckOutByParentAsync(dto.ParentId, staffId, dto.Notes);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/checkout-by-child", async (CheckOutByChildDto dto, ClaimsPrincipal user, IAttendanceService attendanceService) =>
        {
            var staffIdStr = user.FindFirstValue("uid");
            if (staffIdStr is null) 
                return Results.Unauthorized();

            var staffId = Guid.Parse(staffIdStr);
            var result = await attendanceService.CheckOutByChildAsync(dto.ChildId, staffId, dto.Notes);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");
    }
}