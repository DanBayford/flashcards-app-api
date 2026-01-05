using Flashcards.Api.Common.Interfaces;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Users;

namespace Flashcards.Api.Features.Questions;

public class Question: IHasTimestamps
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; } // FK
    public User User { get; set; } = null!; // Navigation property

    public string Prompt { get; set; } = null!;
    public string? Hint { get; set; }
    public string Answer { get; set; } = null!;
    
    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.VeryLow;
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    
    /*
     * EF core would infer M2M from List<Category> Categories,
     * but explicit join table QuestionCategory is more extensible later if required
     */
    public ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
    
}

// Join table for Question <-> Category many-to-many relationships
public class QuestionCategory
{
    public Guid QuestionId { get; set; } // FK
    public Question Question { get; set; } = null!; // Navigation property
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}