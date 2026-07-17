using System.Reflection;
using FluentValidation;
using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage, ErrorType.Validation))
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        return CreateValidationFailure(ValidationError.FromErrors(errors));
    }

    private static TResponse CreateValidationFailure(ValidationError validationError)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(validationError);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(Result.Failure) && method.IsGenericMethod)
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [validationError])!;
    }
}
