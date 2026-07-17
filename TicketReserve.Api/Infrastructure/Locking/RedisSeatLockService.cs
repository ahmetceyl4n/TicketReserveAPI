using StackExchange.Redis;

namespace TicketReserve.Api.Infrastructure.Locking;

public sealed class RedisSeatLockService : ISeatLockService
{
    private const string KeyPrefix = "seat-lock:";

    // KEYS[1] = kilit key'i, ARGV[1] = beklenen sahibin userId'si.
    // GET+karşılaştır+DEL'i tek script içinde çalıştırıyoruz: ayrı komutlar olsaydı, GET ile DEL
    // arasındaki milisaniyelerde TTL dolup başka bir kullanıcı aynı key'i kilitleyebilirdi — biz
    // de kendi kilidimiz sanıp başkasının kilidini silmiş olurduk. Redis bir Lua script'ini tek,
    // bölünemez bir komut gibi çalıştırır; script sürerken araya başka komut giremez.
    private const string ReleaseIfOwnedScript = """
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            return redis.call('DEL', KEYS[1])
        else
            return 0
        end
        """;

    private readonly IConnectionMultiplexer _redis;

    public RedisSeatLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> TryLockAsync(IReadOnlyCollection<Guid> seatIds, string userId, TimeSpan ttl)
    {
        var db = _redis.GetDatabase();
        var acquired = new List<Guid>(seatIds.Count);

        foreach (var seatId in seatIds)
        {
            var wasSet = await db.StringSetAsync(BuildKey(seatId), userId, ttl, When.NotExists);

            if (wasSet)
            {
                acquired.Add(seatId);
                continue;
            }

            // All-or-nothing: bu çağrıda buraya kadar alınmış kilitleri geri bırak. Bunlar
            // saniyeler önce bizim tarafımızdan set edildiği için doğrudan silinebilir —
            // ReleaseAsync'teki Lua sahiplik kontrolü, çok daha sonra (kullanıcı iptal ettiğinde
            // veya TTL'e yakın) gelen release istekleri için gerekli, burada değil.
            foreach (var lockedSeatId in acquired)
            {
                await db.KeyDeleteAsync(BuildKey(lockedSeatId));
            }

            return false;
        }

        return true;
    }

    public async Task<bool> ReleaseAsync(IReadOnlyCollection<Guid> seatIds, string userId)
    {
        var db = _redis.GetDatabase();
        var allReleased = true;

        foreach (var seatId in seatIds)
        {
            var result = (int)await db.ScriptEvaluateAsync(
                ReleaseIfOwnedScript,
                [BuildKey(seatId)],
                [userId]);

            allReleased &= result == 1;
        }

        return allReleased;
    }

    public async Task<bool> AreAllLockedByAsync(IReadOnlyCollection<Guid> seatIds, string userId)
    {
        var db = _redis.GetDatabase();
        var keys = seatIds.Select(id => (RedisKey)BuildKey(id)).ToArray();
        var values = await db.StringGetAsync(keys);

        return values.All(value => value.HasValue && value == userId);
    }

    public async Task<IReadOnlySet<Guid>> GetLockedSeatIdsAsync(IReadOnlyCollection<Guid> seatIds)
    {
        if (seatIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var db = _redis.GetDatabase();
        var seatIdArray = seatIds.ToArray();
        var keys = seatIdArray.Select(id => (RedisKey)BuildKey(id)).ToArray();
        var values = await db.StringGetAsync(keys);

        var locked = new HashSet<Guid>();
        for (var i = 0; i < seatIdArray.Length; i++)
        {
            if (values[i].HasValue)
            {
                locked.Add(seatIdArray[i]);
            }
        }

        return locked;
    }

    private static string BuildKey(Guid seatId) => $"{KeyPrefix}{seatId}";
}
