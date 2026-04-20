using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Progress.Commands.MakeChoice;
using OnlineBookAdventures.Application.Features.Progress.Commands.ResetProgress;
using OnlineBookAdventures.Application.Features.Progress.Commands.StartStory;
using OnlineBookAdventures.Application.Features.Progress.Queries.GetProgress;
using OnlineBookAdventures.Domain.Entities;
using OnlineBookAdventures.Infrastructure.Persistence;
using ProgressEntity = OnlineBookAdventures.Domain.Entities.Progress;

namespace OnlineBookAdventures.Tests.Progress;

/// <summary>
/// Unit tests for progress tracking handlers.
/// </summary>
public sealed class ProgressHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _storyId = Guid.NewGuid();
    private StoryNode _startNode = null!;
    private StoryNode _endNode = null!;
    private Choice _choice = null!;

    public ProgressHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ProgressTests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        SeedStory();
    }

    private void SeedStory()
    {
        var user = new User { Id = _userId, Username = "reader", Email = "reader@test.com", PasswordHash = "hash" };
        var story = new Story { Id = _storyId, AuthorId = _userId, Title = "Test", Description = "Desc" };
        _startNode = new StoryNode { Id = Guid.NewGuid(), StoryId = _storyId, Title = "Start", Content = "Begin", IsStart = true };
        _endNode = new StoryNode { Id = Guid.NewGuid(), StoryId = _storyId, Title = "End", Content = "Done", IsEnd = true };
        _choice = new Choice { Id = Guid.NewGuid(), FromNodeId = _startNode.Id, ToNodeId = _endNode.Id, Label = "Finish", Order = 1 };

        _context.Users.Add(user);
        _context.Stories.Add(story);
        _context.StoryNodes.AddRange(_startNode, _endNode);
        _context.Choices.Add(_choice);
        _context.SaveChanges();
    }

    [Fact]
    public async Task StartStory_CreatesProgressAtStartNode()
    {
        // Arrange
        var handler = new StartStoryCommandHandler(_context, new NoOpEventPublisher());

        // Act
        var nodeId = await handler.Handle(new StartStoryCommand(_userId, _storyId), CancellationToken.None);

        // Assert
        nodeId.Should().Be(_startNode.Id);
        var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == _userId && p.StoryId == _storyId);
        progress.Should().NotBeNull();
        progress!.CurrentNodeId.Should().Be(_startNode.Id);
    }

    [Fact]
    public async Task StartStory_WhenProgressExists_ResetsToStartNode()
    {
        // Arrange
        _context.Progresses.Add(new ProgressEntity { UserId = _userId, StoryId = _storyId, CurrentNodeId = _endNode.Id, IsCompleted = true });
        await _context.SaveChangesAsync();

        var handler = new StartStoryCommandHandler(_context, new NoOpEventPublisher());

        // Act
        await handler.Handle(new StartStoryCommand(_userId, _storyId), CancellationToken.None);

        // Assert
        var progress = await _context.Progresses.FirstAsync(p => p.UserId == _userId && p.StoryId == _storyId);
        progress.CurrentNodeId.Should().Be(_startNode.Id);
        progress.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task MakeChoice_AdvancesToNextNode()
    {
        // Arrange
        _context.Progresses.Add(new ProgressEntity { UserId = _userId, StoryId = _storyId, CurrentNodeId = _startNode.Id });
        await _context.SaveChangesAsync();

        var handler = new MakeChoiceCommandHandler(_context, new NoOpEventPublisher());

        // Act
        var result = await handler.Handle(new MakeChoiceCommand(_userId, _storyId, _choice.Id), CancellationToken.None);

        // Assert
        result.NewNodeId.Should().Be(_endNode.Id);
        result.IsEnd.Should().BeTrue();
    }

    [Fact]
    public async Task MakeChoice_WithInvalidChoice_ThrowsInvalidOperation()
    {
        // Arrange
        _context.Progresses.Add(new ProgressEntity { UserId = _userId, StoryId = _storyId, CurrentNodeId = _endNode.Id });
        await _context.SaveChangesAsync();

        var handler = new MakeChoiceCommandHandler(_context, new NoOpEventPublisher());

        // Act — choice belongs to startNode but current node is endNode
        var act = async () => await handler.Handle(new MakeChoiceCommand(_userId, _storyId, _choice.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetProgress_ReturnsCurrentNode()
    {
        // Arrange
        _context.Progresses.Add(new ProgressEntity { UserId = _userId, StoryId = _storyId, CurrentNodeId = _startNode.Id });
        await _context.SaveChangesAsync();

        var handler = new GetProgressQueryHandler(_context);

        // Act
        var result = await handler.Handle(new GetProgressQuery(_userId, _storyId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentNodeId.Should().Be(_startNode.Id);
    }

    [Fact]
    public async Task ResetProgress_RemovesProgressRecord()
    {
        // Arrange
        _context.Progresses.Add(new ProgressEntity { UserId = _userId, StoryId = _storyId, CurrentNodeId = _startNode.Id });
        await _context.SaveChangesAsync();

        var handler = new ResetProgressCommandHandler(_context);

        // Act
        await handler.Handle(new ResetProgressCommand(_userId, _storyId), CancellationToken.None);

        // Assert
        var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == _userId && p.StoryId == _storyId);
        progress.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();

    private sealed class NoOpEventPublisher : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class => Task.CompletedTask;
    }
}
