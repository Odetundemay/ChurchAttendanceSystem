using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class ParentEndpoints
{
    public static void MapParentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/parents").WithTags("Parents");

        group.MapPost("/", async (CreateParentDto dto, IParentService parentService) =>
        {
            var result = await parentService.CreateParentAsync(dto);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/{id:guid}/qr", async (Guid id, IParentService parentService) =>
        {
            var result = await parentService.GenerateQrCodeAsync(id);
            if (result.IsSuccess)
                return Results.File(result.Data!, "image/png");
            
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");


    }
}