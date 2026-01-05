namespace Flashcards.Api.Common.Auth;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
    );