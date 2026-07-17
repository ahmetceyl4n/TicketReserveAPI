using System.Diagnostics;
using MediatR;
using Serilog.Context;
using TicketReserve.Api.Infrastructure.Correlation;

namespace TicketReserve.Api.Infrastructure.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using (LogContext.PushProperty("CorrelationId", _correlationIdProvider.CorrelationId))
        {
            _logger.LogInformation("İşlem başlıyor: {RequestName}", requestName);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "İşlem tamamlandı: {RequestName} ({ElapsedMilliseconds} ms)",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}
