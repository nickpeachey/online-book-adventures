using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.CreateStory;

/// <summary>
/// Validates the <see cref="CreateStoryCommand"/> inputs.
/// </summary>
public sealed class CreateStoryCommandValidator : AbstractValidator<CreateStoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateStoryCommandValidator"/> class.
    /// </summary>
    public CreateStoryCommandValidator()
    {
        RuleFor(x => x.AuthorId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
