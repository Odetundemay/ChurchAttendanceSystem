using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class ChildEndpoints
{
    public static void MapChildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/parents").WithTags("Children");

        group.MapPost("/{parentId:guid}/children", async (Guid parentId, CreateChildDto dto, IChildService childService) =>
        {
            var result = await childService.CreateChildAsync(parentId, dto);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");
    }
}