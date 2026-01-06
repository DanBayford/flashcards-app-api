using Flashcards.Api.Features.Categories;

namespace Flashcards.Api.Features.Questions;

public sealed record QuestionDto
{
    public Guid Id { get; init; }
    public string Prompt { get; init; } = null!;
    public string? Hint { get; init; }
    public string Answer { get; set; } = null!;
    public ConfidenceLevel Confidence { get; init; }
    public IReadOnlyList<QuestionCategoryDto> Categories { get; init; } = [];
};

// Alternative syntax to below, useful for small DTOs
// public sealed record QuestionCategoryDto(Guid Id, string Name);

public sealed record QuestionCategoryDto
{
   public Guid Id { get; init; }
   public string Name { get; init; } = null!;
}

public sealed record CreateQuestionDto
{
    public string Prompt { get; init; } = null!;
    public string? Hint { get; init; }
    public string Answer { get; init; } = null!;
    public IReadOnlyList<Guid> QuestionCategoryIds { get; init; } = [];
}

public sealed record UpdateQuestionDto
{
    public string Prompt { get; init; } = null!;
    public string? Hint { get; init; }
    public string Answer { get; init; } = null!;
    public ConfidenceLevel Confidence { get; init; }
    public IReadOnlyList<Guid> QuestionCategoryIds { get; init; } = [];
}