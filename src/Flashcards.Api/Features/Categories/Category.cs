using Flashcards.Api.Common.Interfaces;
using Flashcards.Api.Features.Questions;
using Flashcards.Api.Features.Users;

namespace Flashcards.Api.Features.Categories;

public class Category: IHasTimestamps
{
    public Guid Id  { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    
    public ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
}