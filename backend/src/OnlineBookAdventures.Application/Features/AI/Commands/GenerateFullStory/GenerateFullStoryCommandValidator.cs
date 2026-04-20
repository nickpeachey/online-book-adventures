using FluentValidation;

namespace OnlineBookAdventures.Application.Features.AI.Commands.GenerateFullStory;

/// <summary>
/// Validates the <see cref="GenerateFullStoryCommand"/> inputs.
/// </summary>
public sealed class GenerateFullStoryCommandValidator : AbstractValidator<GenerateFullStoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateFullStoryCommandValidator"/> class.
    /// </summary>
    public GenerateFullStoryCommandValidator()
    {
        RuleFor(x => x.AuthorId).NotEmpty();
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(500);
    }
}
