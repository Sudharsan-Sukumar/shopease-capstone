namespace Application.Api.Common.Results;

public record ApiError(string Code, string Message);

/// <summary>Uniform envelope every endpoint returns - {success,data,error,meta}.</summary>
public record ApiResponse<T>(bool Success, T? Data, ApiError? Error, object? Meta = null)
{
    public static ApiResponse<T> Ok(T data, object? meta = null) => new(true, data, null, meta);
    public static ApiResponse<T> Fail(ApiError error) => new(false, default, error);
}
