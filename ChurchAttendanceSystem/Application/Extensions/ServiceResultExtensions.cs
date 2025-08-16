using ChurchAttendanceSystem.Domain;

namespace ChurchAttendanceSystem.Application.Extensions;

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        var apiResult = result.IsSuccess
            ? ApiResult<T>.SuccessResult(result.Data!, result.StatusCode)
            : ApiResult<T>.ErrorResult(result.ErrorMessage!, result.StatusCode);
        
        return Results.Json(apiResult, statusCode: result.StatusCode);
    }

    public static IResult ToHttpResult(this ServiceResult result)
    {
        var apiResult = result.IsSuccess
            ? ApiResult.SuccessResult(result.StatusCode)
            : ApiResult.ErrorResult(result.ErrorMessage!, result.StatusCode);
        
        return Results.Json(apiResult, statusCode: result.StatusCode);
    }
}