namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a Choose Your Own Adventure story.
/// </summary>
public class Story : BaseEntity
{
    /// <summary>Gets or sets the identifier of the author.</summary>
    public Guid AuthorId { get; set; }

    /// <summary>Gets or sets the story title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the story description shown on the browse page.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the story cover image.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Gets or sets a value that indicates whether this story is publicly published.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Gets or sets the date and time when the story was published.</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>Gets or sets the date and time when this entity was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>Gets or sets the author of this story.</summary>
    public User Author { get; set; } = null!;

    /// <summary>Gets or sets the nodes that make up this story.</summary>
    public ICollection<StoryNode> Nodes { get; set; } = [];

    /// <summary>Gets or sets the reader progress records for this story.</summary>
    public ICollection<Progress> Progresses { get; set; } = [];

    /// <summary>Gets or sets the ratings for this story.</summary>
    public ICollection<Rating> Ratings { get; set; } = [];

    /// <summary>Gets or sets the comments for this story.</summary>
    public ICollection<Comment> Comments { get; set; } = [];
}
