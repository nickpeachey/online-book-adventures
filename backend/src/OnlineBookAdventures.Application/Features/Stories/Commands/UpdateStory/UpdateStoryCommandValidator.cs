using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UpdateStory;

/// <summary>
/// Validates the <see cref="UpdateStoryCommand"/> inputs.
/// </summary>
public sealed class UpdateStoryCommandValidator : AbstractValidator<UpdateStoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStoryCommandValidator"/> class.
    /// </summary>
    public UpdateStoryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
