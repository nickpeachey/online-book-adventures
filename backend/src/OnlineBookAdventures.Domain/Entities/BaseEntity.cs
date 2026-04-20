namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Base class for all domain entities providing a strongly-typed identifier.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets or sets the unique identifier for this entity.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the date and time when this entity was created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
