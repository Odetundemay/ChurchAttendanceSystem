using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class ScanEndpoints
{
    public static void MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("QR Scanning");

        group.MapPost("/scan", async (ScanDto dto, IParentService parentService) =>
        {
            var result = await parentService.ScanQrCodeAsync(dto);
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");
    }
}