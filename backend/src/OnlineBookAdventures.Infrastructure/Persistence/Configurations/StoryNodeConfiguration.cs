using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="StoryNode"/> entity.
/// </summary>
internal sealed class StoryNodeConfiguration : IEntityTypeConfiguration<StoryNode>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<StoryNode> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Content)
            .IsRequired();

        builder.HasOne(n => n.Story)
            .WithMany(s => s.Nodes)
            .HasForeignKey(n => n.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.StoryId);
        builder.HasIndex(n => new { n.StoryId, n.IsStart });
    }
}
