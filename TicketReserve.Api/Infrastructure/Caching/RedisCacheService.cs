using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace TicketReserve.Api.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _cache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var json = await _cache.GetStringAsync(key, ct);

            if (json is null) return default;

            return JsonSerializer.Deserialize<T>(json);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            return _cache.RemoveAsync(key, ct);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultTtl
            };
            var json = JsonSerializer.Serialize(value);

            await _cache.SetStringAsync(key, json, options, ct);
        }

        public async Task<object?> GetAsync(string key, Type type, CancellationToken ct = default)
        {
            var json = await _cache.GetStringAsync(key, ct);

            return json is null ? null : JsonSerializer.Deserialize(json, type);
        }
    }
}
