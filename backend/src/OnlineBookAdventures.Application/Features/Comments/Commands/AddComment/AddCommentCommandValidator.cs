using FluentValidation;

namespace OnlineBookAdventures.Application.Features.Comments.Commands.AddComment;

/// <summary>
/// Validates the <see cref="AddCommentCommand"/> inputs.
/// </summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddCommentCommandValidator"/> class.
    /// </summary>
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
    }
}
