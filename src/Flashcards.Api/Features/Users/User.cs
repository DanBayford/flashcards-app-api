using Flashcards.Api.Common.Interfaces;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Questions;

namespace Flashcards.Api.Features.Users;

public class User : IHasTimestamps
{

    public Guid Id { get; set; }

    // Credentials
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    
    // Refresh token
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
    
    // Metadata
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    
    // Navigation properties
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}