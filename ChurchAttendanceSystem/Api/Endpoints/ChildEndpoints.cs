using ChurchAttendanceSystem.Application.Interfaces;
using ChurchAttendanceSystem.Application.Extensions;
using ChurchAttendanceSystem.Dto;

namespace ChurchAttendanceSystem.Api.Endpoints;

public static class ChildEndpoints
{
    public static void MapChildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/children").WithTags("Children");

        group.MapPost("/list", async (IChildService childService) =>
        {
            var result = await childService.GetChildrenAsync();
            return result.ToHttpResult();
        }).RequireAuthorization("Staff");

        group.MapDelete("/{id:guid}", async (Guid id, IChildService childService) =>
        {
            var result = await childService.DeleteChildAsync(id);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");

        // Also add under parents for creating children
        var parentGroup = app.MapGroup("/api/parents").WithTags("Children");
        parentGroup.MapPost("/{parentId:guid}/children", async (Guid parentId, CreateChildDto dto, IChildService childService) =>
        {
            var result = await childService.CreateChildAsync(parentId, dto);
            return result.ToHttpResult();
        }).RequireAuthorization("AdminOnly");
    }
}