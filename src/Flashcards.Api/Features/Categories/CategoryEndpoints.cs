using System.Security.Claims;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Features.Categories;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapUserCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/category").WithTags("Category");
        
        group.MapGet("", GetCategoriesAsync).RequireAuthorization();
        group.MapGet("/{CategoryId:guid}", GetCategoryByIdAsync).RequireAuthorization();
        group.MapPost("", CreateCategoryAsync).RequireAuthorization();
        group.MapPut("/{CategoryId:guid}", UpdateCategoryAsync).RequireAuthorization();
        group.MapDelete("/{CategoryId:guid}", DeleteCategoryAsync).RequireAuthorization();
        
        return app;
    }

    private static async Task<IResult> GetCategoriesAsync(
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
    )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Note use of expression tree 'Project' to map to multiple DTOs at DB level
        var categories = await db.Categories
            .Where(c => c.UserId == user.Id)
            .Select(CategoryMappings.Project)
            .ToListAsync();
        
        return Results.Ok(categories);
    }
    
    private static async Task<IResult> GetCategoryByIdAsync(
        Guid categoryId,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
    )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.User == user);

        if (category == null)
        {
            return Results.NotFound();
        }

        var response = category.ToDto();

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCategoryAsync(
        CreateCategoryDto request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
    )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Confirm a category (associated with request user) with this title doesn't already exist
        var exists = await db.Categories
            .AnyAsync(c =>
                c.User == user &&
                c.Name == request.Name
            );
        
        if (exists)
        {
            return Results.BadRequest(new { error = "Category already exists" });
        }
        
        // Add new category
        var category = request.CreateFromDto(user);
        db.Categories.Add(category);
        
        await db.SaveChangesAsync();
        
        // Construct response
        var response = category.ToDto();
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateCategoryAsync(
        Guid categoryId,
        UpdateCategoryDto request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db)
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.User == user);
        
        if (category == null)
        {
            return Results.NotFound();
        }
        
        category.UpdateFromDto(request);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCategoryAsync(
        Guid categoryId,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
    )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        Category? category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.User == user);

        if (category == null)
        {
            return Results.NotFound();
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}