namespace Flashcards.Api.Common.Auth;

public sealed class CookieService(IHostEnvironment env) : ICookieService
{
    public CookieOptions RefreshCookies() => new()
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(), // Secure on prod
        SameSite = env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None, // Cannot be non-secure AND SameSiteMode.None
        Path = "/",
        MaxAge = TimeSpan.FromDays(28),
    };
    
}