using System.Security.Claims;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user").WithTags("User"); // WithTags is for doc generation only

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapGet("/me",  GetUserInfoAsync).RequireAuthorization();
        group.MapPost("/refresh", RefreshTokenAsync);
        group.MapPost("/logout", LogOutAsync).RequireAuthorization();
        group.MapPost("/password", ChangePasswordAsync).RequireAuthorization();
        
        return app;
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext context,
        RegisterRequest request,
        ApplicationDbContext db,
        IPasswordService passwordService,
        IJwtTokenService tokenService,
        ICookieService cookieService)
    {
        // Check if email already exists - use of AsNoTracking as we will not be editing this entity if found (minor performance boost)
        var existing = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (existing != null)
        {
            return Results.BadRequest(new { error = "Email already exists" });
        }

        // Confirm password strength
        var (passwordOk, errors) = passwordService.VerifyPasswordStrength(request.Password);
        if (!passwordOk)
        {
            return Results.BadRequest(new { errors });
        }
        
        // Create user object
        var salt = passwordService.GenerateSalt();
        var hash = passwordService.HashPassword(request.Password, salt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordSalt = salt,
            PasswordHash = hash,
            RefreshToken = null,
            RefreshTokenExpiresAtUtc = DateTime.MinValue
        };
        
        db.Users.Add(user);
        
        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = DateTime.UtcNow + tokenService.RefreshTokenLifetime;
        
        await db.SaveChangesAsync();

        var response = new AuthResponse(
            AccessToken: accessToken,
            UserInfo: user.ToDto()
        );
        
        // Add refresh token to cookie
        context.Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            cookieService.RefreshCookies()
            );

        return Results.Ok(response);
    }
    
    
    private static async Task<IResult> LoginAsync(
        HttpContext context,
        LoginRequest request,
        ApplicationDbContext db,
        IPasswordService passwordService,
        IJwtTokenService tokenService,
        ICookieService cookieService
    )
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
     
        // Check for existing user
        if (user == null)
        {
            // Don't leak existing/non existing emails data
            return Results.BadRequest(new { error = "Invalid credentials" });
        }

        // Confirm supplied password
        var passwordOk = passwordService.VerifyPassword(
            request.Password, 
            user.PasswordHash, 
            user.PasswordSalt
        );
     
        if (!passwordOk)
        {
            return Results.BadRequest(new { error = "Invalid credentials" });
        }
     
        // Create appropriate tokens and save to user table
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = DateTime.UtcNow + tokenService.RefreshTokenLifetime;
        await db.SaveChangesAsync();

        // Generate response object
        var response = new AuthResponse(
            AccessToken: accessToken,
            UserInfo: user.ToDto()
        );
        
        // Add refresh token to cookie
        context.Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            cookieService.RefreshCookies()
            );

        return Results.Ok(response);
    }

    private static async Task<IResult> GetUserInfoAsync(
        ClaimsPrincipal userPrincipal, 
        ApplicationDbContext db
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        // Use DTO extension method
        var response = user.ToDto();
        return Results.Ok(response);
    }
    
    
    private static async Task<IResult> RefreshTokenAsync(
        HttpContext context,
        RefreshRequest request,
        ApplicationDbContext db,
        IJwtTokenService tokenService,
        ICookieService cookieService
        )
    {
        var token = context.Request.Cookies["refresh_token"];

        // No refresh token
        if (string.IsNullOrEmpty(token)) 
        {
            return Results.Unauthorized();
        }
        
        var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);
        var now = DateTime.UtcNow;

        // Token not associated with user or token expired
        if (user == null || user.RefreshTokenExpiresAtUtc <= now)
        {
            return Results.Unauthorized();
        }
        
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = now + tokenService.RefreshTokenLifetime;
        await db.SaveChangesAsync();

        var response = new AuthResponse(
            AccessToken: accessToken,
            UserInfo: user.ToDto()
        );
        
        context.Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            cookieService.RefreshCookies());
        
        return Results.Ok(response);
    }

    /*
     * Authentication in Program.cs configures JWT middleware.
     * For authenticated requests, the middleware reads the
     * Authorization: Bearer  header, validates the JWT,
     * and populates HttpContext.User (ClaimsPrincipal) with the token’s claims.
     */
    
    private static async Task<IResult> LogOutAsync(
        HttpContext context,
        ClaimsPrincipal userPrincipal, 
        ApplicationDbContext db,
        ICookieService cookieService
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        user.RefreshToken = null;
        user.RefreshTokenExpiresAtUtc = DateTime.MinValue;
        
        await db.SaveChangesAsync();
        
        // Clear refresh cookie
        context.Response.Cookies.Delete(
            "refresh_token",
            cookieService.RefreshCookies()
            );

        return Results.NoContent();
    }

    private static async Task<IResult> ChangePasswordAsync(
        HttpContext context,
        ChangePasswordRequest request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db,
        IPasswordService passwordService,
        IJwtTokenService tokenService,
        ICookieService cookieService
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Confirm current password
        var passwordOk = passwordService.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash,
            user.PasswordSalt
        );
        
        if (!passwordOk)
        {
            return Results.BadRequest(new { error = "Invalid credentials" });
        }

        // Confirm new password
        var (newPasswordOk, errors ) = passwordService.VerifyPasswordStrength(request.NewPassword);
        if (!newPasswordOk)
        {
            return Results.BadRequest(new { errors });
        }
        
        // Set new password
        var salt = passwordService.GenerateSalt();
        var hash = passwordService.HashPassword(request.NewPassword, salt);
        
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        
        // Update tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = DateTime.UtcNow + tokenService.RefreshTokenLifetime;
        
        await db.SaveChangesAsync();
        
        // Create response object with new tokens
        var response = new AuthResponse(
            AccessToken: accessToken,
            UserInfo: user.ToDto()
        );
        
        context.Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            cookieService.RefreshCookies());
        
        return Results.Ok(response);
    }
}