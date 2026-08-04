namespace TicketReserve.Api.Infrastructure.Caching
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan? Expiration => null;
    }
}
