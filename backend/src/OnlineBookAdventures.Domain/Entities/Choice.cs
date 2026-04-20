namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a choice (edge) connecting two story nodes.
/// </summary>
public class Choice : BaseEntity
{
    /// <summary>Gets or sets the identifier of the node this choice originates from.</summary>
    public Guid FromNodeId { get; set; }

    /// <summary>Gets or sets the identifier of the node this choice leads to.</summary>
    public Guid ToNodeId { get; set; }

    /// <summary>Gets or sets the label displayed to the reader for this choice.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order of this choice relative to other choices on the same node.</summary>
    public int Order { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the node this choice originates from.</summary>
    public StoryNode FromNode { get; set; } = null!;

    /// <summary>Gets or sets the node this choice leads to.</summary>
    public StoryNode ToNode { get; set; } = null!;
}
