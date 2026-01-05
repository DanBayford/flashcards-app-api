using Flashcards.Api.Common.Interfaces;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Questions;
using Flashcards.Api.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.Api.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Expose the tables to the application code via EF/LINQ interface
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionCategory> QuestionCategories => Set<QuestionCategory>();

    // Runs on app start - allows EF to understand your database and build an in-memory model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scan current assembly (ie the web api) for classes implementing IEntityTypeConfiguration interface
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Manually configure QuestionCategory many-to-many table (allows for potential expansion of table later)
        modelBuilder.Entity<QuestionCategory>()
            .ToTable("question_categories");
        
        modelBuilder.Entity<QuestionCategory>()
            .HasKey(qc => new { qc.QuestionId, qc.CategoryId });
        
        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Question)
            .WithMany(q => q.QuestionCategories)
            .HasForeignKey(qc => qc.QuestionId)
            .OnDelete(DeleteBehavior.Cascade); // Remove related join row if Question deleted

        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Category)
            .WithMany(c => c.QuestionCategories)
            .HasForeignKey(qc => qc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // Remove related join row(s) if Category deleted

        base.OnModelCreating(modelBuilder);
    }
    
    // Automatically update timestamp information on all entities that implement IHasTimestamps
    // This adds extra functionality to EF Core's SaveChangesAsync method
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IHasTimestamps>();

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.UpdatedAtUtc = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}