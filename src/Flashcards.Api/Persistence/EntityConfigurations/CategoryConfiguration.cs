using Flashcards.Api.Features.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flashcards.Api.Persistence.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table name
        builder.ToTable("categories");
        
        // Primary key
        builder.HasKey(c => c.Id);
        
        // Column constraints
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        // Confirm Category.Name unique to user
        builder.HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();
        
        // Relationships
        builder.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}