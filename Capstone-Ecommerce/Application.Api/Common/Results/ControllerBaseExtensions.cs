using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Common.Results;

/// <summary>Maps a Result/Result&lt;T&gt; to the right HTTP status + ApiResponse envelope, in one call per controller action.</summary>
public static class ControllerBaseExtensions
{
    public static IActionResult ToApiResponse<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<T>.Ok(result.Value));

        return MapFailure<T>(controller, result.Error);
    }

    public static IActionResult ToApiResponse(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<object?>.Ok(null));

        return MapFailure<object?>(controller, result.Error);
    }

    private static IActionResult MapFailure<T>(ControllerBase controller, Error error)
    {
        var response = ApiResponse<T>.Fail(new ApiError(error.Code, error.Message));

        return error.Type switch
        {
            ErrorType.Validation => controller.BadRequest(response),
            ErrorType.NotFound => controller.NotFound(response),
            ErrorType.Conflict => controller.Conflict(response),
            ErrorType.Unauthorized => controller.Unauthorized(response),
            ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, response),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, response),
        };
    }
}
