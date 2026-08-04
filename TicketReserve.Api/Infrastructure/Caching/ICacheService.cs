namespace TicketReserve.Api.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key,CancellationToken ct = default);
        Task<object?> GetAsync(string key, Type type, CancellationToken ct = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);
            
    }
}
