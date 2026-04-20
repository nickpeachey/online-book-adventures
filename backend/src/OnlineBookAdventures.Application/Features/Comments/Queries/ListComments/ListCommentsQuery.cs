using MediatR;

namespace OnlineBookAdventures.Application.Features.Comments.Queries.ListComments;

/// <summary>
/// Query to retrieve paginated comments for a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of comments per page.</param>
public record ListCommentsQuery(Guid StoryId, int Page = 1, int PageSize = 20) : IRequest<ListCommentsResult>;

/// <summary>
/// A paginated list of comments.
/// </summary>
/// <param name="Comments">The comments on the current page.</param>
/// <param name="TotalCount">The total number of comments.</param>
/// <param name="Page">The current page number.</param>
/// <param name="PageSize">The page size.</param>
public record ListCommentsResult(IReadOnlyList<CommentDto> Comments, int TotalCount, int Page, int PageSize);

/// <summary>
/// Represents a single comment.
/// </summary>
/// <param name="Id">The comment identifier.</param>
/// <param name="UserId">The author's identifier.</param>
/// <param name="Username">The author's username.</param>
/// <param name="Body">The comment text.</param>
/// <param name="CreatedAt">When the comment was posted.</param>
/// <param name="EditedAt">When the comment was last edited, or <see langword="null"/> if never edited.</param>
public record CommentDto(Guid Id, Guid UserId, string Username, string Body, DateTimeOffset CreatedAt, DateTimeOffset? EditedAt);
