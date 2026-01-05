using System.Security.Claims;
using Flashcards.Api.Features.Users;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Common.Auth;

public static class AuthHelpers
{
    /// <summary>
    /// Tries to extract the userId from the JWT and find the User in the db
    /// </summary>
    /// <param name="userPrincipal">The in-memory user object built fron the validated JWT</param>
    /// <param name="db">The ApplicationDbContext</param>
    /// <returns>user || null</returns>
    public static async Task<User?> GetUserFromToken(ClaimsPrincipal userPrincipal, ApplicationDbContext db)
    {
        // Extract userId from JWT
        var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var guid))
        {
            return null;
        }
        
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == guid);
        return user ?? null;
    }
}