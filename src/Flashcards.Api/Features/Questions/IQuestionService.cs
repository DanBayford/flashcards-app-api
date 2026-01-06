namespace Flashcards.Api.Features.Questions;

public interface IQuestionService
{
    Task UpdateQuestionCategoriesAsync(Question question, IEnumerable<Guid> questionCategoryIds);
}