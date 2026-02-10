using System.Security.Claims;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Features.Questions;

public static class QuestionEndpoints
{
    public static IEndpointRouteBuilder MapQuestionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/question").WithTags("Question");

        group.MapGet("/", GetQuestionsAsync).RequireAuthorization();
        group.MapPost("/", CreateQuestionAsync).RequireAuthorization();
        group.MapPut("/{QuestionId:guid}", UpdateQuestionAsync).RequireAuthorization();
        group.MapDelete("/{QuestionId:guid}", DeleteQuestionAsync).RequireAuthorization();

        return app;
    }
    
    private static async Task<IResult> GetQuestionsAsync(
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db,
        Guid[]? categoryId,
        bool hideMastered = false,
        int page = 1
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Pagination config
        int pageSize = 12;
        page = Math.Max(page, 1);

        // Base query
        var query = db.Questions
            .Where(q => q.UserId == user.Id);

        // Create empty category array if not in params
        categoryId ??= Array.Empty<Guid>();
        
        if (categoryId.Length > 0)
        {
            query = query.Where(q => q.QuestionCategories.Any(qc => categoryId.Contains(qc.CategoryId)));
        }

        if (hideMastered)
        {
            query = query.Where(q => q.Confidence < ConfidenceLevel.Low);
        }

        var totalCount = await query.CountAsync();
            
        // Make DB call    
        var questions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(q => q.CreatedAtUtc) // Order after manipulating IQueryable
            .Select(QuestionMappings.Project)
            .ToListAsync();

        // Create response object to include pagination metadata
        var response = new
        {
            questions,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateQuestionAsync(
        CreateQuestionDto request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db)
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Create new Question entity
        var question = request.CreateFromDto(user);

        // Associate any Category instances via QuestionCategories
        var userCategories = await db.Categories
            .Where(c => request.QuestionCategoryIds.Contains(c.Id) &&  c.UserId == user.Id)
            .ToListAsync();

        foreach (var category in userCategories)
        {
            question.QuestionCategories.Add(new QuestionCategory
            {
                QuestionId = question.Id,
                CategoryId = category.Id
            });
        }

        db.Questions.Add(question);
        await db.SaveChangesAsync();

        // Go back to DB to get associated Categories as Dto's via projection
        var response = await db.Questions
            .Where(q => q.Id == question.Id)
            .Select(QuestionMappings.Project)
            .FirstOrDefaultAsync();
        
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionDto request,
        ClaimsPrincipal userPrincipal,
        IQuestionService questionService,
        ApplicationDbContext db)
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == questionId && q.User == user);
        if (question == null)
        {
            return Results.NotFound();
        }

        // Update the scalar fields by DTO
        question.UpdateFromDto(request);
        
        // Update QuestionCategories via QuestionService
        await questionService
            .UpdateQuestionCategoriesAsync(question, request.QuestionCategoryIds);
        
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuestionAsync(
        Guid questionId,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        Question? question = await db.Questions.FirstOrDefaultAsync(q => q.Id == questionId && q.User == user);

        if (question == null)
        {
            return Results.NotFound();
        }

        db.Questions.Remove(question);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}