namespace OnlineBookAdventures.Application.Common.Interfaces;

public interface IStoryGenerationService
{
    Task<GeneratedStoryGraph> GenerateFullStoryAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> SuggestNodeContentAsync(string storyTitle, string nodeTitle, string currentContent, CancellationToken cancellationToken = default);
}

public record GeneratedStoryNode(string Title, string Content, bool IsStart, bool IsEnd, double PositionX, double PositionY);
public record GeneratedStoryChoice(int FromNodeIndex, int ToNodeIndex, string Label, int Order);
public record GeneratedStoryGraph(string Title, string Description, IReadOnlyList<GeneratedStoryNode> Nodes, IReadOnlyList<GeneratedStoryChoice> Choices);
