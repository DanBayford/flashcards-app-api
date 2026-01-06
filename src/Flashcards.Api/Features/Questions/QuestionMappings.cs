using System.Linq.Expressions;
using Flashcards.Api.Features.Users;

namespace Flashcards.Api.Features.Questions;

public static class QuestionMappings
{

    public static QuestionDto ToDto(this Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Prompt = question.Prompt,
            Hint = question.Hint,
            Answer = question.Answer,
            Confidence = question.Confidence
        };
    }

    public static Question CreateFromDto(this CreateQuestionDto dto, User user)
    {
        var question = new Question
        {
            Prompt = dto.Prompt,
            Hint = dto.Hint,
            Answer = dto.Answer,
            Confidence = ConfidenceLevel.VeryLow,
            UserId = user.Id
        };

        return question;
    }

    public static void UpdateFromDto(this Question question, UpdateQuestionDto dto)
    {
        question.Prompt = dto.Prompt;
        question.Hint = dto.Hint;
        question.Answer = dto.Answer;
        question.Confidence = dto.Confidence;

    }

    public static readonly Expression<Func<Question, QuestionDto>> Project = q => 
        new QuestionDto
        {
            Id = q.Id,
            Prompt =  q.Prompt,
            Hint = q.Hint,
            Answer = q.Answer,
            Confidence = q.Confidence,
            Categories = q.QuestionCategories
                .Select(qc => new QuestionCategoryDto{
                    Id = qc.CategoryId, 
                    Name = qc.Category.Name
                    })
                .ToList()
        };
}