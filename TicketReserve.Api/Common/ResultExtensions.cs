namespace TicketReserve.Api.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess ? Results.Ok() : CreateProblem(result.Error);
    }

    public static IResult ToHttpResult<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess ? Results.Ok(result.Value) : CreateProblem(result.Error);
    }

    private static IResult CreateProblem(Error error)
    {
        var errorCodeExtension = new Dictionary<string, object?> { ["errorCode"] = error.Code };

        if (error is ValidationError validationError)
        {
            var validationErrors = validationError.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(group => group.Key, group => group.Select(e => e.Message).ToArray());

            return Results.ValidationProblem(validationErrors, extensions: errorCodeExtension);
        }

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode,
            extensions: errorCodeExtension);
    }
}
