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

    public static readonly Expression<Func<Category, CategoryDto>> Project = c =>
        new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        };
}