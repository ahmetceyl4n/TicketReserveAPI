namespace TicketReserve.Api.Infrastructure.Caching
{
    // Cache key formatlarının TEK kaynağı. Hem query (yazan) hem invalidation (silen) buradan çağırır ki
    // key string'i iki yerde elle yazılıp birbirinden kaymasın (kaysa invalidation sessizce çalışmaz).
    public static class CacheKeys
    {
        public static string Event(Guid id) => $"event:{id}";
    }
}
