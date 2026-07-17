using System.Security.Claims;

namespace TicketReserve.Api.Infrastructure.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Sadece .RequireAuthorization() ile korunan bir endpoint'in isteği içinde çağrılmalı —
    // aksi hâlde HttpContext ya null'dur ya da kimliksizdir, bu durumda bilerek patlıyoruz
    // (sessizce boş string dönmek, bir bug'ı "unknown" gibi anlamsız bir kullanıcıya mal eder).
    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "Kullanıcı kimliği bulunamadı — ICurrentUserService yalnızca authenticated bir istek bağlamında kullanılabilir.");
}
