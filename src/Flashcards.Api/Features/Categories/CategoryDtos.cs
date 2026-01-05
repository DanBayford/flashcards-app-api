namespace Flashcards.Api.Features.Categories;

public sealed record CategoryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public sealed record CreateCategoryDto
{
    public required string Name { get; init; }
};

public sealed record UpdateCategoryDto
{
    public required string Name { get; init; }
}
