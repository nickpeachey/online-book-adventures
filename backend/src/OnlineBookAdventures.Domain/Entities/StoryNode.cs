namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a single node (scene or page) within a story.
/// </summary>
public class StoryNode : BaseEntity
{
    /// <summary>Gets or sets the identifier of the story this node belongs to.</summary>
    public Guid StoryId { get; set; }

    /// <summary>Gets or sets the node title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the narrative content displayed to the reader.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets a value that indicates whether this is the starting node of the story.</summary>
    public bool IsStart { get; set; }

    /// <summary>Gets or sets a value that indicates whether this is a terminal (ending) node.</summary>
    public bool IsEnd { get; set; }

    /// <summary>Gets or sets the display position X coordinate for the visual builder.</summary>
    public double PositionX { get; set; }

    /// <summary>Gets or sets the display position Y coordinate for the visual builder.</summary>
    public double PositionY { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the story this node belongs to.</summary>
    public Story Story { get; set; } = null!;

    /// <summary>Gets or sets the choices that originate from this node.</summary>
    public ICollection<Choice> OutgoingChoices { get; set; } = [];

    /// <summary>Gets or sets the choices that lead to this node.</summary>
    public ICollection<Choice> IncomingChoices { get; set; } = [];
}
