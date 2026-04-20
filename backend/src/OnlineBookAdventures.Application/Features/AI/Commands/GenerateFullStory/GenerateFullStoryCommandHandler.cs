using MediatR;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.AI.Commands.GenerateFullStory;

/// <summary>
/// Handles <see cref="GenerateFullStoryCommand"/> by generating a full story graph via AI
/// and persisting it to the database.
/// </summary>
public sealed class GenerateFullStoryCommandHandler(
    IStoryGenerationService storyGenerationService,
    IApplicationDbContext context)
    : IRequestHandler<GenerateFullStoryCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(GenerateFullStoryCommand request, CancellationToken cancellationToken)
    {
        var graph = await storyGenerationService
            .GenerateFullStoryAsync(request.Prompt, cancellationToken)
            .ConfigureAwait(false);

        var story = new Story
        {
            AuthorId = request.AuthorId,
            Title = graph.Title,
            Description = graph.Description
        };

        context.Stories.Add(story);

        // Map index → Guid so choices can reference nodes by index
        var nodeIdMap = new Dictionary<int, Guid>(graph.Nodes.Count);

        for (var i = 0; i < graph.Nodes.Count; i++)
        {
            var generatedNode = graph.Nodes[i];
            var node = new StoryNode
            {
                StoryId = story.Id,
                Title = generatedNode.Title,
                Content = generatedNode.Content,
                IsStart = generatedNode.IsStart,
                IsEnd = generatedNode.IsEnd,
                PositionX = generatedNode.PositionX,
                PositionY = generatedNode.PositionY
            };

            context.StoryNodes.Add(node);
            nodeIdMap[i] = node.Id;
        }

        foreach (var generatedChoice in graph.Choices)
        {
            // Skip choices with out-of-range indices (AI occasionally uses 1-based indexing)
            if (!nodeIdMap.ContainsKey(generatedChoice.FromNodeIndex) ||
                !nodeIdMap.ContainsKey(generatedChoice.ToNodeIndex))
                continue;

            var choice = new Choice
            {
                FromNodeId = nodeIdMap[generatedChoice.FromNodeIndex],
                ToNodeId = nodeIdMap[generatedChoice.ToNodeIndex],
                Label = generatedChoice.Label,
                Order = generatedChoice.Order
            };

            context.Choices.Add(choice);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return story.Id;
    }
}
