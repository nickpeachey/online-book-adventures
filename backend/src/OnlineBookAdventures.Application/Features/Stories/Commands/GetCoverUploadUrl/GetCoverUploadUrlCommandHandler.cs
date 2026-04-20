using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.GetCoverUploadUrl;

/// <summary>
/// Handles the <see cref="GetCoverUploadUrlCommand"/> by generating a pre-signed upload URL.
/// </summary>
public sealed class GetCoverUploadUrlCommandHandler(
    IApplicationDbContext context,
    IStorageService storageService,
    IConfiguration configuration) : IRequestHandler<GetCoverUploadUrlCommand, CoverUploadUrlResult>
{
    private const string BucketName = "story-covers";

    /// <inheritdoc/>
    public async Task<CoverUploadUrlResult> Handle(GetCoverUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can upload a cover image.");

        var extension = request.FileExtension.TrimStart('.').ToLowerInvariant();
        var objectKey = $"{request.StoryId}/cover.{extension}";

        var uploadUrl = await storageService
            .GetPresignedUrlAsync(BucketName, objectKey, expiryMinutes: 15)
            .ConfigureAwait(false);

        var minioUrl = configuration.GetConnectionString("MinIO") ?? "http://localhost:9000";
        var publicUrl = $"{minioUrl.TrimEnd('/')}/{BucketName}/{objectKey}";

        return new CoverUploadUrlResult(uploadUrl, objectKey, publicUrl);
    }
}
