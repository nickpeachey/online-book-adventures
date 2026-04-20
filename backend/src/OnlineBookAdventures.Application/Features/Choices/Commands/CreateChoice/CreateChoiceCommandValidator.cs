using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.CreateChoice;

/// <summary>
/// Validates the <see cref="CreateChoiceCommand"/> inputs.
/// </summary>
public sealed class CreateChoiceCommandValidator : AbstractValidator<CreateChoiceCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateChoiceCommandValidator"/> class.
    /// </summary>
    public CreateChoiceCommandValidator()
    {
        RuleFor(x => x.FromNodeId).NotEmpty();
        RuleFor(x => x.ToNodeId).NotEmpty()
            .NotEqual(x => x.FromNodeId).WithMessage("A choice cannot point to its own source node.");
        RuleFor(x => x.Label).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Order).GreaterThan(0);
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
