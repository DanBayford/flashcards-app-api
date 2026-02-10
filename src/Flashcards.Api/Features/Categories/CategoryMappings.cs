using System.Linq.Expressions;
using Flashcards.Api.Features.Users;

namespace Flashcards.Api.Features.Categories;

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            QuestionCount = category.QuestionCategories.Count(cq => cq.Question.UserId == category.UserId),
        };
    }

    public static Category CreateFromDto(this CreateCategoryDto dto, User user)
    {
        var category =  new Category
        {
            Name = dto.Name,
            User = user,
            UserId = user.Id
        };

        return category;
    }

    public static void UpdateFromDto(this Category category,  UpdateCategoryDto dto)
    {
        category.Name = dto.Name;
    }

    public static Expression<Func<Category, CategoryDto>> Project => category =>
        new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            // Annotation to count number of questions user has on each category
            QuestionCount = category.QuestionCategories.Count(qc => qc.Question.UserId == category.UserId),
        };
}