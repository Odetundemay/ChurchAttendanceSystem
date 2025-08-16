namespace ChurchAttendanceSystem.Domain;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult<T> Success(T data, int statusCode = 200)
        => new(true, data, null, statusCode);

    public static ServiceResult<T> Failure(string errorMessage, int statusCode = 400)
        => new(false, default, errorMessage, statusCode);

    public static ServiceResult<T> NotFound(string errorMessage = "Resource not found")
        => new(false, default, errorMessage, 404);

    public static ServiceResult<T> Conflict(string errorMessage)
        => new(false, default, errorMessage, 409);

    public static ServiceResult<T> Unauthorized(string errorMessage = "Unauthorized")
        => new(false, default, errorMessage, 401);
}

public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }

    private ServiceResult(bool isSuccess, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult Success(int statusCode = 200)
        => new(true, null, statusCode);

    public static ServiceResult Failure(string errorMessage, int statusCode = 400)
        => new(false, errorMessage, statusCode);
    
    public static ServiceResult NotFound(string errorMessage = "Resource not found")
        => new(false, errorMessage, 404);

}