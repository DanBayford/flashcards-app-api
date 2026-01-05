using Flashcards.Api.Common.Auth;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Questions;
using Flashcards.Api.Features.Users;
using Flashcards.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Data.Reset;

public class ResetTestUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IHostEnvironment _env;
    private readonly ILogger _logger;
    
    public ResetTestUserService(
        ApplicationDbContext context, 
        IPasswordService passwordService, 
        IHostEnvironment env, 
        ILogger<ResetTestUserService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _env = env;
        _logger = logger;
    }

    public async Task TestDbConnection()
    {
        // Note this method requires DOTNET_ENVIRONMENT=Development in your dev environment
        _logger.LogInformation($"Environment: {_env.EnvironmentName}");
        if (_env.IsDevelopment())
        {
            _logger.LogInformation("In dev mode - execute TestDbConnection");
            var conn = _context.Database.GetDbConnection();

            // Confirm DB parameters
            _logger.LogInformation("Provider:      " + conn.GetType().FullName);
            _logger.LogInformation("ConnectionStr: " + conn.ConnectionString);
            _logger.LogInformation("Database:      " + conn.Database);

            // Confirm connection available
            var canConnect = await _context.Database.CanConnectAsync();
            _logger.LogInformation("CanConnect:    " + canConnect);
        }
    }

    public async Task ResetTestUserAsync()
    {
        const string testUserEmail = "dan@flashcards.com";
        const string testUserPassword = "P@ssword!";
        var testUserPasswordSalt = _passwordService.GenerateSalt();
        var testUserPasswordHash = _passwordService.HashPassword(testUserPassword, testUserPasswordSalt);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == testUserEmail);
        
        if (user == null)
        {
            _logger.LogInformation($"Test user not found - creating test user {testUserEmail}");
            // Create Test User
            user = new User
            {
                Email = "dan@flashcards.com",
                PasswordHash = testUserPasswordHash,
                PasswordSalt = testUserPasswordSalt,
            };
            await _context.Users.AddAsync(user);
        }
        else
        {
            _logger.LogInformation($"Test user {user.Email} found - confirming password");
            // Reset password in case changed in demo
            user.PasswordHash = testUserPasswordHash;
            user.PasswordSalt = testUserPasswordSalt;
        }

        // Use DB transaction
        await using var tx = await _context.Database.BeginTransactionAsync();

        await ClearTestUserAsync(user);
        
        await SeedTestUserAsync(user);
        
        // Save changes and commit transaction
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task ClearTestUserAsync(User user)
    {
        var questions = await _context.Questions
            .Where(q => q.UserId == user.Id)
            .ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.UserId == user.Id)
            .ToListAsync();

        // Remove join rows in QuestionCategory table explicitly (safe even without cascade)
        if (questions.Count > 0 || categories.Count > 0)
        {
            var questionIds = questions.Select(q => q.Id).ToList();
            var categoryIds = categories.Select(c => c.Id).ToList();

            var joins = await _context.QuestionCategories
                .Where(qc =>
                    questionIds.Contains(qc.QuestionId) ||
                    categoryIds.Contains(qc.CategoryId))
                .ToListAsync();

            _context.QuestionCategories.RemoveRange(joins);
        }

        // Remove questions & categories
        _context.Questions.RemoveRange(questions);
        _context.Categories.RemoveRange(categories);
        
    }

    private Task SeedTestUserAsync(User user)
    {
        // Create test user categories
        var c1 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "JavaScript"
        };
        var c2 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Python"
        };
        var c3 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Django"
        };
        
        // Create test user questions
        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is hoisting in JavaScript?",
            Hint = "Think in terms of function declarations versus function expressions",
            Answer =
                "Hoisting is JavaScript's behaviour of moving declarations (but not expressions) to the top of the scope during complication (ie before running). This means, for example, you can call functions before their declarations appear in the script."
        };

        var q2 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What are the main 4 Python collections, which are mutable and which are index based?",
            Hint = "",
            Answer = "The index based collections are lists (mutable) and tuples (immutable). Sets are mutable and unordered, with unique values. Dictionaries are mutable and are ordered by insertion, but not index based - you access by the key value."
        };

        var q3 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What are the two types of view in a Django application",
            Hint = "",
            Answer = "Django supports both FBV (function based views) and CBV (class based views).",
        };
        
        /*
         * Create relevant joins in QuestionCategory
         * Note using entities rather than explicitly using ID's - IDs don't exist in DB yet (transaction) so FK constraint will fail
         * This pattern lets EF work it out
         */  
        var joins = new List<QuestionCategory>
        {
            new QuestionCategory { Question = q1, Category = c1 },
            new QuestionCategory { Question = q2, Category = c2 },
            // note q3 has two categories
            new QuestionCategory { Question = q3, Category = c2 },
            new QuestionCategory { Question = q3, Category = c3 }
        };
        
        // Add to relevant tables
        _context.Categories.AddRange(c1, c2);
        _context.Questions.AddRange(q1, q2, q3);
        _context.QuestionCategories.AddRange(joins);

        // method doesn't actually call await anywhere so return Task.CompletedTask to fit method signature
        return Task.CompletedTask;
    }
}