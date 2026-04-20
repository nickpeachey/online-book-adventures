using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds development data into the database on application startup.
/// </summary>
public static class DevDataSeeder
{
    /// <summary>
    /// Seeds a sample story with nodes and choices if no stories exist.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await context.Stories.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        logger.LogInformation("Seeding development data...");

        var authorId = Guid.NewGuid();
        var author = new User
        {
            Id = authorId,
            Username = "demo_author",
            Email = "author@example.com",
            PasswordHash = "$2a$11$placeholder_hash_for_dev_only"
        };

        var storyId = Guid.NewGuid();
        var story = new Story
        {
            Id = storyId,
            AuthorId = authorId,
            Title = "The Enchanted Forest",
            Description = "A mysterious forest holds many secrets. Which path will you take?",
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow
        };

        var startNode = new StoryNode
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "The Forest Edge",
            Content = "You stand at the edge of an ancient forest. Two paths stretch before you — one lit by golden sunlight, the other shrouded in shadow.",
            IsStart = true,
            PositionX = 0,
            PositionY = 0
        };

        var sunnyNode = new StoryNode
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "The Sunlit Glade",
            Content = "The golden path leads you to a beautiful glade. A friendly fox sits beside a bubbling spring.",
            PositionX = -200,
            PositionY = 150
        };

        var shadowNode = new StoryNode
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "The Dark Thicket",
            Content = "The shadowed path grows darker. You hear rustling in the undergrowth...",
            PositionX = 200,
            PositionY = 150
        };

        var endGoodNode = new StoryNode
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "A New Friend",
            Content = "The fox leads you to a hidden village of woodland creatures. You have found a home away from home. The End.",
            IsEnd = true,
            PositionX = -200,
            PositionY = 300
        };

        var endBadNode = new StoryNode
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            Title = "Lost in the Dark",
            Content = "The shadows close around you. You wander for hours before finding your way back to the start. The End.",
            IsEnd = true,
            PositionX = 200,
            PositionY = 300
        };

        var choices = new List<Choice>
        {
            new() { FromNodeId = startNode.Id, ToNodeId = sunnyNode.Id, Label = "Follow the sunlit path", Order = 1 },
            new() { FromNodeId = startNode.Id, ToNodeId = shadowNode.Id, Label = "Take the shadowed path", Order = 2 },
            new() { FromNodeId = sunnyNode.Id, ToNodeId = endGoodNode.Id, Label = "Follow the fox", Order = 1 },
            new() { FromNodeId = sunnyNode.Id, ToNodeId = startNode.Id, Label = "Turn back", Order = 2 },
            new() { FromNodeId = shadowNode.Id, ToNodeId = endBadNode.Id, Label = "Press deeper into the dark", Order = 1 },
            new() { FromNodeId = shadowNode.Id, ToNodeId = startNode.Id, Label = "Retreat to safety", Order = 2 },
        };

        context.Users.Add(author);
        context.Stories.Add(story);
        context.StoryNodes.AddRange(startNode, sunnyNode, shadowNode, endGoodNode, endBadNode);
        context.Choices.AddRange(choices);

        await context.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("Development seed data applied successfully.");
    }
}
