using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Features.Questions;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _db;
    
    public QuestionService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// This method updates a Questions linked Category instances
    /// These entities are M2M and so the join table needs to be maintained when updating
    /// <param name="question">The Question instance being tracked by EF</param>
    /// <param name="questionCategoryIds">A list of CategoryIds</param>
    /// </summary>
    public async Task UpdateQuestionCategoriesAsync(
        Question question,
        IEnumerable<Guid> questionCategoryIds
        )
    {
        // A normalized and unique list of the IDs of the desired final Category's for the Question
        var finalCategoryIds = questionCategoryIds
            .Distinct()
            .ToHashSet();
        
        // Load the existing QuestionCategory rows for this Question
        var existingRows = await _db.QuestionCategories
            .Where(qc => qc.QuestionId == question.Id)
            .ToListAsync();
        
        // A list of the existing CategoryIDs from the table
        var existingCategoryIds = existingRows
            .Select(qc => qc.CategoryId)
            .ToHashSet();
        
        // Add new entries to QuestionCategory table if required (ie not already in table)
        var toAddIds = finalCategoryIds.Except(existingCategoryIds);

        var rowsToAdd = toAddIds.Select(categoryId =>
            new QuestionCategory
            {
                CategoryId = categoryId,
                QuestionId = question.Id
            });
        
        _db.QuestionCategories.AddRange(rowsToAdd);
        
        // Remove orphaned entries from QuestionCategory table required (ie no longer associated with Question)
        var rowsToRemove = existingRows
            .Where(qc => !finalCategoryIds.Contains(qc.CategoryId));
        
        _db.QuestionCategories.RemoveRange(rowsToRemove);
        
        // Note db save required in caller
    }
}