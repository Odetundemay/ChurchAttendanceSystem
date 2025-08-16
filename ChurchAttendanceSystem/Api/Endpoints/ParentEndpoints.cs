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

        group.MapPost("/list", async (IParentService parentService) =>
        {
            var result = await parentService.GetParentsAsync();
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapPost("/qr", async (QrRequestDto dto, IParentService parentService) =>
        {
            var result = await parentService.GetQrDataAsync(dto.Id);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");


    }
}