namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a user's comment on a story.
/// </summary>
public class Comment : BaseEntity
{
    /// <summary>Gets or sets the identifier of the user who posted this comment.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the identifier of the story this comment belongs to.</summary>
    public Guid StoryId { get; set; }

    /// <summary>Gets or sets the comment body text.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time when this comment was last edited.</summary>
    public DateTimeOffset? EditedAt { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the user who posted this comment.</summary>
    public User User { get; set; } = null!;

    /// <summary>Gets or sets the story this comment belongs to.</summary>
    public Story Story { get; set; } = null!;
}
