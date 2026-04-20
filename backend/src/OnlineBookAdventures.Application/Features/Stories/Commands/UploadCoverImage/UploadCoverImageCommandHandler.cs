using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UploadCoverImage;

/// <summary>
/// Handles the <see cref="UploadCoverImageCommand"/> by uploading the image to storage and updating the story.
/// </summary>
public sealed class UploadCoverImageCommandHandler(
    IApplicationDbContext context,
    IStorageService storageService) : IRequestHandler<UploadCoverImageCommand, string>
{
    private const string BucketName = "story-covers";

    /// <inheritdoc/>
    public async Task<string> Handle(UploadCoverImageCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can upload a cover image.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var objectKey = $"{request.StoryId}/cover{extension}";

        var url = await storageService.UploadAsync(
            BucketName, objectKey, request.FileContent, request.ContentType, cancellationToken)
            .ConfigureAwait(false);

        story.CoverImageUrl = url;
        story.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return url;
    }
}
