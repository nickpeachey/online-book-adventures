using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Comments.Commands.AddComment;
using OnlineBookAdventures.Application.Features.Comments.Commands.DeleteComment;
using OnlineBookAdventures.Application.Features.Comments.Queries.ListComments;
using OnlineBookAdventures.Application.Features.Ratings.Commands.RateStory;
using OnlineBookAdventures.Application.Features.Ratings.Queries.GetStoryRating;
using OnlineBookAdventures.Domain.Entities;
using OnlineBookAdventures.Infrastructure.Persistence;

namespace OnlineBookAdventures.Tests.Social;

/// <summary>
/// Unit tests for rating and comment handlers.
/// </summary>
public sealed class SocialHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _storyId = Guid.NewGuid();

    public SocialHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"SocialTests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        SeedData();
    }

    private void SeedData()
    {
        _context.Users.Add(new User { Id = _userId, Username = "user1", Email = "u1@test.com", PasswordHash = "hash" });
        _context.Stories.Add(new Story { Id = _storyId, AuthorId = _userId, Title = "Story", Description = "Desc" });
        _context.SaveChanges();
    }

    // ── Rating Tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RateStory_CreatesRating()
    {
        // Arrange
        var handler = new RateStoryCommandHandler(_context, new NoOpEventPublisher());

        // Act
        await handler.Handle(new RateStoryCommand(_userId, _storyId, 4), CancellationToken.None);

        // Assert
        var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == _userId && r.StoryId == _storyId);
        rating.Should().NotBeNull();
        rating!.Score.Should().Be(4);
    }

    [Fact]
    public async Task RateStory_WhenRatingExists_UpdatesScore()
    {
        // Arrange
        _context.Ratings.Add(new Rating { UserId = _userId, StoryId = _storyId, Score = 3 });
        await _context.SaveChangesAsync();

        var handler = new RateStoryCommandHandler(_context, new NoOpEventPublisher());

        // Act
        await handler.Handle(new RateStoryCommand(_userId, _storyId, 5), CancellationToken.None);

        // Assert
        var ratings = await _context.Ratings.Where(r => r.UserId == _userId && r.StoryId == _storyId).ToListAsync();
        ratings.Should().HaveCount(1);
        ratings[0].Score.Should().Be(5);
    }

    [Fact]
    public async Task GetStoryRating_ReturnsAverageAndUserScore()
    {
        // Arrange
        var user2 = Guid.NewGuid();
        _context.Ratings.AddRange(
            new Rating { UserId = _userId, StoryId = _storyId, Score = 4 },
            new Rating { UserId = user2, StoryId = _storyId, Score = 2 }
        );
        await _context.SaveChangesAsync();

        var handler = new GetStoryRatingQueryHandler(_context);

        // Act
        var result = await handler.Handle(new GetStoryRatingQuery(_storyId, _userId), CancellationToken.None);

        // Assert
        result.TotalRatings.Should().Be(2);
        result.AverageScore.Should().Be(3.0);
        result.UserScore.Should().Be(4);
    }

    // ── Comment Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddComment_PersistsComment()
    {
        // Arrange
        var handler = new AddCommentCommandHandler(_context);

        // Act
        var id = await handler.Handle(new AddCommentCommand(_userId, _storyId, "Great story!"), CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        var comment = await _context.Comments.FindAsync(id);
        comment.Should().NotBeNull();
        comment!.Body.Should().Be("Great story!");
    }

    [Fact]
    public async Task DeleteComment_ByAuthor_RemovesComment()
    {
        // Arrange
        var comment = new Comment { UserId = _userId, StoryId = _storyId, Body = "To delete" };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context);

        // Act
        await handler.Handle(new DeleteCommentCommand(comment.Id, _userId), CancellationToken.None);

        // Assert
        var deleted = await _context.Comments.FindAsync(comment.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteComment_ByNonAuthor_ThrowsUnauthorized()
    {
        // Arrange
        var comment = new Comment { UserId = _userId, StoryId = _storyId, Body = "Mine" };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context);

        // Act
        var act = async () => await handler.Handle(new DeleteCommentCommand(comment.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ListComments_ReturnsPaginatedResults()
    {
        // Arrange
        _context.Comments.AddRange(
            new Comment { UserId = _userId, StoryId = _storyId, Body = "First" },
            new Comment { UserId = _userId, StoryId = _storyId, Body = "Second" },
            new Comment { UserId = _userId, StoryId = _storyId, Body = "Third" }
        );
        await _context.SaveChangesAsync();

        var handler = new ListCommentsQueryHandler(_context);

        // Act
        var result = await handler.Handle(new ListCommentsQuery(_storyId, Page: 1, PageSize: 2), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Comments.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();

    private sealed class NoOpEventPublisher : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class => Task.CompletedTask;
    }
}
