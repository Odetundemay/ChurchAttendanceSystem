using ChurchAttendanceSystem.Domain;

namespace ChurchAttendanceSystem.Application.Extensions;

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                201 => Results.Created("", result.Data),
                _ => Results.Ok(result.Data)
            };
        }

        return result.StatusCode switch
        {
            404 => Results.NotFound(new { error = result.ErrorMessage }),
            409 => Results.Conflict(new { error = result.ErrorMessage }),
            401 => Results.Json(new { error = result.ErrorMessage }, statusCode: 401),
            _ => Results.BadRequest(new { error = result.ErrorMessage })
        };
    }

    public static IResult ToHttpResult(this ServiceResult result)
    {
        if (result.IsSuccess)
            return Results.Ok(new { success = true });

        return result.StatusCode switch
        {
            404 => Results.NotFound(new { error = result.ErrorMessage }),
            409 => Results.Conflict(new { error = result.ErrorMessage }),
            401 => Results.Json(new { error = result.ErrorMessage }, statusCode: 401),
            _ => Results.BadRequest(new { error = result.ErrorMessage })
        };
    }
}