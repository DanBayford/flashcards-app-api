using System.Security.Claims;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Features.Questions;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Features.Quizzes;

public static class QuizEndpoints
{
    public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/quiz").WithTags("Quiz");
        
        group.MapPost("/generate", GenerateQuizAsync).RequireAuthorization();
        group.MapPost("/update", UpdateQuizAsync).RequireAuthorization();
        
        return app;
    }

    private static async Task<IResult> GenerateQuizAsync(
        GenerateQuizDto request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Base query for User Questions
        IQueryable<Question> query = db.Questions.Where(q => q.User == user);
        
        // Remove mastered Questions from Quiz if required (ie ConfidenceLevel == 5)
        if (!request.IncludeMastered)
        {
            query = query.Where(q => q.Confidence < ConfidenceLevel.VeryHigh);
        }

        // Filter by Category if provided
        if (request.QuestionCategoryIds.Count > 0)
        {
            var categoryIds = request.QuestionCategoryIds.ToHashSet(); // Hash set optimizes EF
            
            query = query
                .Where(q => q.QuestionCategories.Any(qc 
                    => categoryIds.Contains(qc.CategoryId))
                );
        }

        var questions = await query
            .Select(QuestionMappings.Project)
            .ToListAsync();

        return Results.Ok(questions);
    }

    private static async Task<IResult> UpdateQuizAsync(
        UpdateQuizDto request,
        ClaimsPrincipal userPrincipal,
        ApplicationDbContext db
        )
    {
        var user = await AuthHelpers.GetUserFromToken(userPrincipal, db);
        if (user == null)
        {
            return Results.Unauthorized();
        }
        
        // Confirm at least one update
        var updates = request.UpdatedQuestions?.ToList() ?? [];
        if (updates.Count == 0)
        {
            return Results.BadRequest(new { error = "No questions supplied to update." });
        }
        
        // Get distinct questionID's from the request
        var requestedIds = updates
            .Select(u => u.QuestionId)
            .Distinct()
            .ToList();

        // Load User Questions matching request questionID's
        var questions = await db.Questions
            .Where(q => q.UserId == user.Id && requestedIds.Contains(q.Id))
            .ToListAsync();

        // Build a lookup of QuestionId -> NewConfidence (last one wins if duplicates)
        var updatesById = updates
            .GroupBy(u => u.QuestionId)
            .ToDictionary(
                g => g.Key,
                g => g.Last().NewConfidenceLevel
            );

        // Update Confidence on each Question
        foreach (var question in questions)
        {
            if (updatesById.TryGetValue(question.Id, out var newConfidence))
            {
                question.Confidence = newConfidence;
            }
        }

        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
}