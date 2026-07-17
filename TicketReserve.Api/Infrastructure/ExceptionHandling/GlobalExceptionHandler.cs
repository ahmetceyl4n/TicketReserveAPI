using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketReserve.Api.Infrastructure.Correlation;

namespace TicketReserve.Api.Infrastructure.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _environment = environment;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationIdProvider = httpContext.RequestServices.GetRequiredService<ICorrelationIdProvider>();

        _logger.LogError(
            exception,
            "Beklenmedik bir hata oluştu. CorrelationId: {CorrelationId}",
            correlationIdProvider.CorrelationId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Beklenmedik bir hata oluştu",
                Detail = _environment.IsDevelopment()
                    ? exception.ToString()
                    : "İstek işlenirken beklenmedik bir sorun oluştu."
            }
        });
    }
}
