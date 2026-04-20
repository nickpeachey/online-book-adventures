namespace OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

/// <summary>
/// Full story graph including all nodes and choices for the reader/builder.
/// </summary>
/// <param name="Story">The story metadata.</param>
/// <param name="Nodes">All nodes in the story.</param>
/// <param name="Choices">All choices connecting the nodes.</param>
public record StoryGraphDto(StoryDto Story, IReadOnlyList<NodeDto> Nodes, IReadOnlyList<ChoiceDto> Choices);

/// <summary>
/// Represents a single story node.
/// </summary>
/// <param name="Id">The node identifier.</param>
/// <param name="StoryId">The owning story identifier.</param>
/// <param name="Title">The node title.</param>
/// <param name="Content">The narrative content.</param>
/// <param name="IsStart">Whether this is the starting node.</param>
/// <param name="IsEnd">Whether this is a terminal node.</param>
/// <param name="PositionX">X coordinate for the visual builder.</param>
/// <param name="PositionY">Y coordinate for the visual builder.</param>
public record NodeDto(
    Guid Id,
    Guid StoryId,
    string Title,
    string Content,
    bool IsStart,
    bool IsEnd,
    double PositionX,
    double PositionY);

/// <summary>
/// Represents a choice (edge) connecting two nodes.
/// </summary>
/// <param name="Id">The choice identifier.</param>
/// <param name="FromNodeId">The source node identifier.</param>
/// <param name="ToNodeId">The destination node identifier.</param>
/// <param name="Label">The choice label shown to the reader.</param>
/// <param name="Order">The display order.</param>
public record ChoiceDto(Guid Id, Guid FromNodeId, Guid ToNodeId, string Label, int Order);
