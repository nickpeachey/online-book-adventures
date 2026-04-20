using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UploadCoverImage;

/// <summary>
/// Command to upload a cover image for a story and update its URL.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="RequestingUserId">The identifier of the requesting user (must be story author).</param>
/// <param name="FileContent">The image file content stream.</param>
/// <param name="ContentType">The MIME type of the image (e.g. image/jpeg).</param>
/// <param name="FileName">The original file name, used to derive the object key extension.</param>
public record UploadCoverImageCommand(
    Guid StoryId,
    Guid RequestingUserId,
    Stream FileContent,
    string ContentType,
    string FileName) : IRequest<string>;
