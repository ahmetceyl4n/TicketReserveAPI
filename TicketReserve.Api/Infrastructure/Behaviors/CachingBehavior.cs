using System.Diagnostics;
using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Caching;

namespace TicketReserve.Api.Infrastructure.Behaviors
{
    public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheableQuery
        where TResponse : Result
    {
        // Kendi span üreticimiz — Npgsql/MassTransit'in hazır ActivitySource'unun elle yazılmış hali.
        private static readonly ActivitySource ActivitySource = new("TicketReserve.Caching");

        private readonly ICacheService _cache;

        public CachingBehavior(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];

            using var activity = ActivitySource.StartActivity($"cache {request.CacheKey}");

            var cachedValue = await _cache.GetAsync(request.CacheKey, valueType, cancellationToken);

            if (cachedValue is not null)
            {
                activity?.SetTag("cache.hit", true);
                return (TResponse)(object)Result.Success((dynamic)cachedValue);
            }

            activity?.SetTag("cache.hit", false);
            var response = await next();

            if (response.IsSuccess)
                await _cache.SetAsync(request.CacheKey, ((dynamic)response).Value, request.Expiration, cancellationToken);

            return response;
        }
    }
}
