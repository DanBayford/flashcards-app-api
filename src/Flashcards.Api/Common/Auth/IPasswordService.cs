namespace Flashcards.Api.Common.Auth;

public interface IPasswordService
{
    string GenerateSalt();
    string HashPassword(string password, string salt);
    bool VerifyPassword(string password, string hash, string salt);
    (bool passwordOk, List<string> errors) VerifyPasswordStrength(string password);
}