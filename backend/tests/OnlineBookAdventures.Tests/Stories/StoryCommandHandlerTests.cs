using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Commands.CreateStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.DeleteStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.PublishStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.UpdateStory;
using OnlineBookAdventures.Application.Features.Stories.Queries.GetStory;
using OnlineBookAdventures.Application.Features.Stories.Queries.ListStories;
using OnlineBookAdventures.Application.Features.Stories.Queries.GetStoryGraph;
using OnlineBookAdventures.Domain.Entities;
using OnlineBookAdventures.Infrastructure.Persistence;

namespace OnlineBookAdventures.Tests.Stories;

/// <summary>
/// Unit tests for story command and query handlers.
/// </summary>
public sealed class StoryCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Guid _authorId = Guid.NewGuid();

    public StoryCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"StoryTests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);

        _context.Users.Add(new User
        {
            Id = _authorId,
            Username = "author",
            Email = "author@test.com",
            PasswordHash = "hash"
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateStory_WithValidData_ReturnsNewGuid()
    {
        // Arrange
        var handler = new CreateStoryCommandHandler(_context);
        var command = new CreateStoryCommand(_authorId, "My Story", "A great adventure");

        // Act
        var id = await handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        var story = await _context.Stories.FindAsync(id);
        story.Should().NotBeNull();
        story!.Title.Should().Be("My Story");
    }

    [Fact]
    public async Task UpdateStory_ByNonAuthor_ThrowsUnauthorized()
    {
        // Arrange
        var story = new Story { AuthorId = _authorId, Title = "Old Title", Description = "Desc" };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var handler = new UpdateStoryCommandHandler(_context);
        var command = new UpdateStoryCommand(story.Id, Guid.NewGuid(), "New Title", "New Desc");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PublishStory_SetsPublishedAtAndIsPublished()
    {
        // Arrange
        var story = new Story { AuthorId = _authorId, Title = "Draft", Description = "Desc" };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var handler = new PublishStoryCommandHandler(_context);
        var command = new PublishStoryCommand(story.Id, _authorId, Publish: true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Stories.FindAsync(story.Id);
        updated!.IsPublished.Should().BeTrue();
        updated.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteStory_ByAuthor_RemovesStory()
    {
        // Arrange
        var story = new Story { AuthorId = _authorId, Title = "To Delete", Description = "Desc" };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var handler = new DeleteStoryCommandHandler(_context);

        // Act
        await handler.Handle(new DeleteStoryCommand(story.Id, _authorId), CancellationToken.None);

        // Assert
        var deleted = await _context.Stories.FindAsync(story.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetStory_WithValidId_ReturnsDto()
    {
        // Arrange
        var story = new Story { AuthorId = _authorId, Title = "Test Story", Description = "Desc" };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var handler = new GetStoryQueryHandler(_context);

        // Act
        var result = await handler.Handle(new GetStoryQuery(story.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Story");
    }

    [Fact]
    public async Task ListStories_ReturnsOnlyPublished()
    {
        // Arrange
        _context.Stories.AddRange(
            new Story { AuthorId = _authorId, Title = "Published", Description = "Desc", IsPublished = true, PublishedAt = DateTimeOffset.UtcNow },
            new Story { AuthorId = _authorId, Title = "Draft", Description = "Desc", IsPublished = false }
        );
        await _context.SaveChangesAsync();

        var handler = new ListStoriesQueryHandler(_context);

        // Act
        var result = await handler.Handle(new ListStoriesQuery(), CancellationToken.None);

        // Assert
        result.Stories.Should().AllSatisfy(s => s.IsPublished.Should().BeTrue());
        result.Stories.Any(s => s.Title == "Draft").Should().BeFalse();
    }

    [Fact]
    public async Task GetStoryGraph_ReturnsNodesAndChoices()
    {
        // Arrange
        var story = new Story { AuthorId = _authorId, Title = "Graph Story", Description = "Desc" };
        var node1 = new StoryNode { StoryId = story.Id, Title = "N1", Content = "Content 1", IsStart = true };
        var node2 = new StoryNode { StoryId = story.Id, Title = "N2", Content = "Content 2", IsEnd = true };
        var choice = new Choice { FromNodeId = node1.Id, ToNodeId = node2.Id, Label = "Go", Order = 1 };

        _context.Stories.Add(story);
        _context.StoryNodes.AddRange(node1, node2);
        _context.Choices.Add(choice);
        await _context.SaveChangesAsync();

        var handler = new GetStoryGraphQueryHandler(_context, new NoOpCache());

        // Act
        var result = await handler.Handle(new GetStoryGraphQuery(story.Id), CancellationToken.None);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.Choices.Should().HaveCount(1);
        result.Choices[0].Label.Should().Be("Go");
    }

    public void Dispose() => _context.Dispose();

    private sealed class NoOpCache : IStoryGraphCache
    {
        public Task<OnlineBookAdventures.Application.Features.Stories.Queries.Dtos.StoryGraphDto?> GetAsync(Guid storyId, CancellationToken cancellationToken = default) => Task.FromResult<OnlineBookAdventures.Application.Features.Stories.Queries.Dtos.StoryGraphDto?>(null);
        public Task SetAsync(Guid storyId, OnlineBookAdventures.Application.Features.Stories.Queries.Dtos.StoryGraphDto graph, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task InvalidateAsync(Guid storyId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
