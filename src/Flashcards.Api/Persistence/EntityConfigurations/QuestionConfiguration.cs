using Flashcards.Api.Features.Questions;
using Flashcards.Api.Features.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flashcards.Api.Persistence.EntityConfigurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        // Table name
        builder.ToTable("questions");
        
        // Primary key
        builder.HasKey(q => q.Id);
        
        // Column constraints
        builder.Property(q => q.Prompt)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(q => q.Hint)
            .HasMaxLength(500);
        
        builder.Property(q => q.Answer)
            .IsRequired()
            .HasMaxLength(1000);
        
        // Enum config (confirm stored in DB as int)
        builder.Property(q => q.Confidence)
            .HasConversion<int>();

        // Relationships
        builder.HasOne(q => q.User)
            .WithMany(u => u.Questions)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}