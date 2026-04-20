using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.GetCoverUploadUrl;

/// <summary>
/// Command to generate a pre-signed URL for direct client-side cover image upload.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
/// <param name="ContentType">The MIME type of the image to be uploaded.</param>
/// <param name="FileExtension">The file extension (e.g. .jpg, .png).</param>
public record GetCoverUploadUrlCommand(
    Guid StoryId,
    Guid RequestingUserId,
    string ContentType,
    string FileExtension) : IRequest<CoverUploadUrlResult>;

/// <summary>
/// Contains the pre-signed URL and object key for a direct upload.
/// </summary>
/// <param name="UploadUrl">The pre-signed PUT URL valid for 15 minutes.</param>
/// <param name="ObjectKey">The object key that will be created in the bucket.</param>
/// <param name="PublicUrl">The public URL where the image will be accessible after upload.</param>
public record CoverUploadUrlResult(string UploadUrl, string ObjectKey, string PublicUrl);
