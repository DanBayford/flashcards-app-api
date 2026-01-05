namespace Flashcards.Api.Features.Users;

internal static class UserMappings
{
    public static UserInfoDto ToDto(this User user)
    {
        return new UserInfoDto
        {
            UserId = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAtUtc,
            UpdatedAt = user.UpdatedAtUtc,
        };
    }
}