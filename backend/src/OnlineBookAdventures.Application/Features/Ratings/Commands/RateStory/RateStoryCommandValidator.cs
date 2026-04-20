using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Ratings.Commands.RateStory;

/// <summary>
/// Validates the <see cref="RateStoryCommand"/> inputs.
/// </summary>
public sealed class RateStoryCommandValidator : AbstractValidator<RateStoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateStoryCommandValidator"/> class.
    /// </summary>
    public RateStoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5.");
    }
}
