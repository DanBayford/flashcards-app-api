namespace Flashcards.Api.Common.Auth;

public sealed class CookieService(IHostEnvironment env) : ICookieService
{
    public CookieOptions RefreshCookies() => new()
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(),
        SameSite = SameSiteMode.None,
        Path = "/",
        MaxAge = TimeSpan.FromDays(28),
    };
    
}