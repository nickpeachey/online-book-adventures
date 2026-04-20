using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="Story"/> entity.
/// </summary>
internal sealed class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.CoverImageUrl)
            .HasMaxLength(2048);

        builder.HasOne(s => s.Author)
            .WithMany(u => u.AuthoredStories)
            .HasForeignKey(s => s.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.AuthorId);
        builder.HasIndex(s => s.IsPublished);
    }
}
