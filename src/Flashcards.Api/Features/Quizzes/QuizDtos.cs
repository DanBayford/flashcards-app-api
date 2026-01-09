using Flashcards.Api.Features.Questions;

namespace Flashcards.Api.Features.Quizzes;

public sealed record GenerateQuizDto
{
    public required bool IncludeMastered { get; init; }
    public required IReadOnlyList<Guid> QuestionCategoryIds { get; init; } = [];
}

public sealed record UpdateQuizDto
{
    public required IEnumerable<UpdateQuestionConfidenceDto> UpdatedQuestions { get; init; } = [];
}

public sealed record UpdateQuestionConfidenceDto
{
    public required Guid QuestionId { get; init; }
    public required ConfidenceLevel NewConfidenceLevel { get; init; }
}