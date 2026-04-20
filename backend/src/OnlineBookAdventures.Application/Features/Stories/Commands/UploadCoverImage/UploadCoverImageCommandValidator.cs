using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UploadCoverImage;

/// <summary>
/// Validates the <see cref="UploadCoverImageCommand"/> inputs.
/// </summary>
public sealed class UploadCoverImageCommandValidator : AbstractValidator<UploadCoverImageCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadCoverImageCommandValidator"/> class.
    /// </summary>
    public UploadCoverImageCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, WebP, and GIF images are allowed.");
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FileContent).NotNull();
    }
}
