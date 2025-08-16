namespace ChurchAttendanceSystem.Domain;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult<T> SuccessResult(T data, int statusCode = 200)
        => new() { Success = true, Data = data, StatusCode = statusCode };

    public static ApiResult<T> ErrorResult(string error, int statusCode = 400)
        => new() { Success = false, Error = error, StatusCode = statusCode };
}

public class ApiResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult SuccessResult(int statusCode = 200)
        => new() { Success = true, StatusCode = statusCode };

    public static ApiResult ErrorResult(string error, int statusCode = 400)
        => new() { Success = false, Error = error, StatusCode = statusCode };
}