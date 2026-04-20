using FluentValidation;

namespace OnlineBookAdventures.Application.Features.AI.Commands.SuggestNodeContent;

/// <summary>
/// Validates the <see cref="SuggestNodeContentCommand"/> inputs.
/// </summary>
public sealed class SuggestNodeContentCommandValidator : AbstractValidator<SuggestNodeContentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuggestNodeContentCommandValidator"/> class.
    /// </summary>
    public SuggestNodeContentCommandValidator()
    {
        RuleFor(x => x.NodeTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CurrentContent).MaximumLength(2000);
    }
}
