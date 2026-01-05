namespace Flashcards.Api.Features.Users;

public sealed record UserInfoDto
{
    public required Guid UserId { get; set; }
    public required string Email { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; } 
};