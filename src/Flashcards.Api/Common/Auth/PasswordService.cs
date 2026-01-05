using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Flashcards.Api.Common.Auth;

public class PasswordService : IPasswordService
{
    
    private const int SaltSize = 16; // bytes
    private const int KeySize = 32; // bytes
    private const int Iterations = 100_000;
    
    /// <summary>
    /// Generates a random salt value
    /// Combining a password with a salt before hashing increases security
    /// </summary>
    /// <returns>A random base64 string to use as a salt</returns>
    public string GenerateSalt()
    {
        var saltBytes = new byte[SaltSize];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    /// <summary>
    /// Hash a password in combination with a salt
    /// Hashing algorithms work on bytes, not strings
    /// </summary>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    /// <returns>A base64 string password hash</returns>
    public string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Confirm a plain text password and given salt match a particular hash 
    /// </summary>
    /// <param name="password"></param>
    /// <param name="hash"></param>
    /// <param name="salt"></param>
    /// <returns>A bool verifying a correct plain text password for a given salt and hash</returns>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        var expectedHashBytes = Convert.FromBase64String(hash);
        var saltBytes = Convert.FromBase64String(salt);

        var actualHashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);
        
        return CryptographicOperations.FixedTimeEquals(expectedHashBytes, actualHashBytes);
    }

    /// <summary>
    /// Confirm the strength of a suggested password
    /// </summary>
    /// <param name="password"></param>
    /// <returns>A bool determining is a plain text password is considered strong enough, and a list of validation errors</returns>
    public (bool passwordOk, List<string> errors) VerifyPasswordStrength(string password)
    {
        var errors = new List<string>();

        if (password.Length < 12)
            errors.Add("Password must be at least 12 characters long.");

        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit.");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            errors.Add("Password must contain at least one special character.");

        return (errors.Count == 0, errors);
    }
}