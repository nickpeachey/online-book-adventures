using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.CreateNode;

/// <summary>
/// Validates the <see cref="CreateNodeCommand"/> inputs.
/// </summary>
public sealed class CreateNodeCommandValidator : AbstractValidator<CreateNodeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNodeCommandValidator"/> class.
    /// </summary>
    public CreateNodeCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
    }
}
