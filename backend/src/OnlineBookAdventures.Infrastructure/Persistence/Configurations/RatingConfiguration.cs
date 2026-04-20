using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="Rating"/> entity.
/// </summary>
internal sealed class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        // One rating per user per story
        builder.HasIndex(r => new { r.UserId, r.StoryId }).IsUnique();

        builder.Property(r => r.Score)
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_Rating_Score", "\"Score\" BETWEEN 1 AND 5"));

        builder.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Story)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.StoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
