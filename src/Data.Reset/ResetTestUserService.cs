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
        const string testUserPassword = "P@ssword123!";
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
            Name = "Web Development"
        };
        var c2 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "JavaScript"
        };
        var c3 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "CSS"
        };
        var c4 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "HTML"
        };
        var c5 = new Category
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Programming Concepts"
        };
        
        // Create test user questions
        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What does HTML stand for?",
            Hint = "",
            Answer = "HyperText Markup Language"
        };

        var q2 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the difference between 'let' and 'const' in JavaScript?",
            Hint = "",
            Answer = "'let' allows you to reassign the variable, while 'const' creates a constant reference that cannot be reassigned. Both are block-scoped."
        };

        var q3 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What does CSS stand for?",
            Hint = "",
            Answer = "Cascading Style Sheets",
        };
        
        var q4 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the difference between '==' and '===' in JavaScript?",
            Hint = "",
            Answer = "'==' checks for value equality with type coercion, while '===' checks for both value and type equality (strict equality).",
        };
        
        var q5 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is a closure in JavaScript?",
            Hint = "",
            Answer = "A closure is a function that has access to variables in its outer (enclosing) lexical scope, even after the outer function has returned.",
        };
        
        var q6 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What does DOM stand for?",
            Hint = "",
            Answer = "Document Object Model",
        };
        
        var q7 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is Flexbox used for in CSS?",
            Hint = "",
            Answer = "Flexbox is a CSS layout model that helps distribute space and align items in a container, making it easier to create responsive layouts.",
        };
        
        var q8 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the purpose of the 'async' keyword in JavaScript?",
            Hint = "",
            Answer = "The 'async' keyword declares an asynchronous function that returns a Promise and allows the use of 'await' inside it.",
        };
        
        var q9 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What are semantic HTML elements?",
            Hint = "",
            Answer = "HTML elements that clearly describe their meaning to both the browser and the developer, like <header>, <nav>, <article>, <footer>, etc.",
        };
        
        var q10 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is event bubbling in JavaScript?",
            Hint = "",
            Answer = "Event bubbling is when an event triggered on a child element propagates up through its parent elements in the DOM tree.",
        };
        
        var q11 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the box model in CSS?",
            Hint = "",
            Answer = "The CSS box model describes the rectangular boxes generated for elements, consisting of content, padding, border, and margin.",
        };
        
        var q12 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the difference between null and undefined in JavaScript?",
            Hint = "",
            Answer = "'undefined' means a variable has been declared but not assigned a value. 'null' is an intentional assignment representing no value or empty object.",
        };
        
        var q13 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is CSS Grid?",
            Hint = "",
            Answer = "CSS Grid is a two-dimensional layout system that allows you to create complex responsive layouts using rows and columns."
        };
        
        var q14 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is hoisting in JavaScript?",
            Hint = "",
            Answer = "Hoisting is JavaScript's behavior of moving variable and function declarations to the top of their scope before code execution.",
        };
        
        var q15 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What does the 'viewport' meta tag do?",
            Hint = "",
            Answer = "It controls how a webpage is displayed on mobile devices by setting the viewport width and initial scale for responsive design.",
        };
        
        var q16 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the 'this' keyword in JavaScript?",
            Hint = "",
            Answer = "'this' refers to the object that is executing the current function. Its value depends on how the function is called.",
        };
        
        var q17 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is specificity in CSS?",
            Hint = "",
            Answer = "Specificity determines which CSS rule is applied when multiple rules target the same element. It's calculated based on selector types (inline, IDs, classes, elements).",
        };
        
        var q18 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the difference between a for loop and forEach in JavaScript?",
            Hint = "",
            Answer = "A for loop can be broken or continued with keywords, while forEach cannot. For loops can iterate over any iterable, forEach is only for arrays.",
        };
        
        var q19 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is localStorage in the browser?",
            Hint = "",
            Answer = "localStorage is a web storage API that allows you to store key-value pairs in the browser with no expiration date, persisting even after the browser is closed.",
        };
        
        var q20 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is a Promise in JavaScript?",
            Hint = "",
            Answer = "A Promise is an object representing the eventual completion or failure of an asynchronous operation, with three states: pending, fulfilled, or rejected."
        };
        
        var q21 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the purpose of the 'alt' attribute in an <img> tag?",
            Hint = "",
            Answer = "The 'alt' attribute provides alternative text for an image if it cannot be displayed, and is essential for accessibility and SEO."
        };
        
        var q22 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the spread operator in JavaScript?",
            Hint = "",
            Answer = "The spread operator (...) expands an iterable (like an array) into individual elements, useful for copying arrays or passing multiple arguments."
        };
        
        var q23 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the z-index property in CSS?",
            Hint = "",
            Answer = "The z-index property controls the stacking order of positioned elements, with higher values appearing in front of lower values."
        };
        
        var q24 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is destructuring in JavaScript?",
            Hint = "",
            Answer = "Destructuring is a syntax that allows you to unpack values from arrays or properties from objects into distinct variables in a concise way."
        };
        
        var q25 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What does HTTP stand for?",
            Hint = "",
            Answer = "HyperText Transfer Protocol"
        };
        
        var q26 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is the difference between margin and padding in CSS?",
            Hint = "",
            Answer = "Margin is the space outside an element's border, while padding is the space inside an element's border, between the border and content."
        };
        
        var q27 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is an API?",
            Hint = "",
            Answer = "Application Programming Interface - a set of rules and protocols that allows different software applications to communicate with each other."
        };
        
        var q28 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is a callback function in JavaScript?",
            Hint = "",
            Answer = "A callback function is a function passed as an argument to another function, to be executed after the first function completes."
        };
        
        var q29 = new Question
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Confidence = ConfidenceLevel.VeryLow,
            Prompt = "What is responsive web design?",
            Hint = "",
            Answer = "An approach to web design that makes web pages render well on various devices and screen sizes using flexible layouts, images, and CSS media queries."
        };
        
        
        /*
         * Create relevant joins in QuestionCategory
         * Note using entities rather than explicitly using ID's - IDs don't exist in DB yet (transaction) so FK constraint will fail
         * This pattern lets EF work it out
         */  
        var joins = new List<QuestionCategory>
        {
            new QuestionCategory { Question = q1, Category = c4 },
            new QuestionCategory { Question = q2, Category = c2 },
            new QuestionCategory { Question = q3, Category = c3 },
            new QuestionCategory { Question = q4, Category = c2 },
            new QuestionCategory { Question = q5, Category = c5 },
            // Note Q6 has 2 x Categories
            new QuestionCategory { Question = q6, Category = c1 },
            new QuestionCategory { Question = q6, Category = c4 },
            new QuestionCategory { Question = q7, Category = c3 },
            new QuestionCategory { Question = q8, Category = c2 },
            new QuestionCategory { Question = q9, Category = c4 },  
            new QuestionCategory { Question = q10, Category = c2 },
            new QuestionCategory { Question = q11, Category = c3 },
            new QuestionCategory { Question = q12, Category = c2 },
            new QuestionCategory { Question = q13, Category = c3 },
            new QuestionCategory { Question = q14, Category = c2 },
            new QuestionCategory { Question = q15, Category = c4 },
            new QuestionCategory { Question = q16, Category = c2 },
            new QuestionCategory { Question = q17, Category = c3 },
            new QuestionCategory { Question = q18, Category = c2 },
            new QuestionCategory { Question = q19, Category = c1 },
            new QuestionCategory { Question = q20, Category = c2 },
            new QuestionCategory { Question = q21, Category = c4 },
            new QuestionCategory { Question = q22, Category = c2 },
            new QuestionCategory { Question = q23, Category = c3 },
            new QuestionCategory { Question = q24, Category = c2 },
            new QuestionCategory { Question = q25, Category = c1 },
            new QuestionCategory { Question = q26, Category = c3 },
            new QuestionCategory { Question = q27, Category = c5 },
            new QuestionCategory { Question = q28, Category = c2 },
            new QuestionCategory { Question = q29, Category = c1 },
        };
        
        // Add to relevant tables
        _context.Categories.AddRange(c1, c2, c3, c4, c5);
        _context.Questions.AddRange(q1, q2, q3, q4, q5, q6, q7, q8, q9, q10,  q11, q12, q13, q14, q15, q16, q17, q18, q19, q20, q21, q22, q23, q24, q25, q26, q27, q28, q29);
        _context.QuestionCategories.AddRange(joins);

        // Method doesn't actually call await anywhere so return Task.CompletedTask to fit method signature
        return Task.CompletedTask;
    }
}