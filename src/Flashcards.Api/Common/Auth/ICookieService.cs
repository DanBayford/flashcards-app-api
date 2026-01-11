namespace Flashcards.Api.Common.Auth;

public interface ICookieService
{
    CookieOptions RefreshCookies();
}