using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="Comment"/> entity.
/// </summary>
internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(5000);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Story)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.StoryId);
    }
}
