using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the <see cref="Choice"/> entity.
/// </summary>
internal sealed class ChoiceConfiguration : IEntityTypeConfiguration<Choice>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Choice> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Label)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasOne(c => c.FromNode)
            .WithMany(n => n.OutgoingChoices)
            .HasForeignKey(c => c.FromNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ToNode)
            .WithMany(n => n.IncomingChoices)
            .HasForeignKey(c => c.ToNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.FromNodeId);
        builder.HasIndex(c => c.ToNodeId);
    }
}
