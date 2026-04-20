using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Commands.UploadCoverImage;
using OnlineBookAdventures.Domain.Entities;
using OnlineBookAdventures.Infrastructure.Persistence;

namespace OnlineBookAdventures.Tests.Storage;

/// <summary>
/// Unit tests for cover image upload command handler using a mocked storage service.
/// </summary>
public sealed class StorageCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IStorageService> _storageMock;
    private readonly Guid _authorId = Guid.NewGuid();
    private readonly Guid _storyId = Guid.NewGuid();

    public StorageCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"StorageTests_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _storageMock = new Mock<IStorageService>();
        SeedData();
    }

    private void SeedData()
    {
        _context.Users.Add(new User { Id = _authorId, Username = "author", Email = "a@test.com", PasswordHash = "hash" });
        _context.Stories.Add(new Story { Id = _storyId, AuthorId = _authorId, Title = "Story", Description = "Desc" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task UploadCoverImage_ByAuthor_UpdatesStoryUrlAndReturnsUrl()
    {
        // Arrange
        const string expectedUrl = "http://localhost:9000/story-covers/cover.jpg";
        _storageMock
            .Setup(s => s.UploadAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var handler = new UploadCoverImageCommandHandler(_context, _storageMock.Object);
        var command = new UploadCoverImageCommand(
            _storyId, _authorId, new MemoryStream([0x01, 0x02]), "image/jpeg", "cover.jpg");

        // Act
        var url = await handler.Handle(command, CancellationToken.None);

        // Assert
        url.Should().Be(expectedUrl);
        var story = await _context.Stories.FindAsync(_storyId);
        story!.CoverImageUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task UploadCoverImage_ByNonAuthor_ThrowsUnauthorized()
    {
        // Arrange
        var handler = new UploadCoverImageCommandHandler(_context, _storageMock.Object);
        var command = new UploadCoverImageCommand(
            _storyId, Guid.NewGuid(), new MemoryStream([0x01]), "image/jpeg", "cover.jpg");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _storageMock.Verify(s => s.UploadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadCoverImage_ForNonexistentStory_ThrowsKeyNotFound()
    {
        // Arrange
        var handler = new UploadCoverImageCommandHandler(_context, _storageMock.Object);
        var command = new UploadCoverImageCommand(
            Guid.NewGuid(), _authorId, new MemoryStream([0x01]), "image/jpeg", "cover.jpg");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
