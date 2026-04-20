namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Tracks a reader's current position within a story.
/// </summary>
public class Progress : BaseEntity
{
    /// <summary>Gets or sets the identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the identifier of the story.</summary>
    public Guid StoryId { get; set; }

    /// <summary>Gets or sets the identifier of the node the reader is currently on.</summary>
    public Guid CurrentNodeId { get; set; }

    /// <summary>Gets or sets a value that indicates whether the reader has completed the story.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Gets or sets the date and time when progress was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>Gets or sets the user this progress belongs to.</summary>
    public User User { get; set; } = null!;

    /// <summary>Gets or sets the story this progress belongs to.</summary>
    public Story Story { get; set; } = null!;

    /// <summary>Gets or sets the current story node the reader is on.</summary>
    public StoryNode CurrentNode { get; set; } = null!;
}
