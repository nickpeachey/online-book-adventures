using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="Progress"/> entity.
/// </summary>
internal sealed class ProgressConfiguration : IEntityTypeConfiguration<Progress>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Progress> builder)
    {
        builder.HasKey(p => p.Id);

        // One progress record per user per story
        builder.HasIndex(p => new { p.UserId, p.StoryId }).IsUnique();

        builder.HasOne(p => p.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Story)
            .WithMany(s => s.Progresses)
            .HasForeignKey(p => p.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CurrentNode)
            .WithMany()
            .HasForeignKey(p => p.CurrentNodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
