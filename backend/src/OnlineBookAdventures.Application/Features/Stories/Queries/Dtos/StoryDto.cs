namespace OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

/// <summary>
/// Summary representation of a story for list views.
/// </summary>
/// <param name="Id">The story identifier.</param>
/// <param name="AuthorId">The author's identifier.</param>
/// <param name="AuthorUsername">The author's username.</param>
/// <param name="Title">The story title.</param>
/// <param name="Description">The story description.</param>
/// <param name="CoverImageUrl">The optional cover image URL.</param>
/// <param name="IsPublished">Whether the story is published.</param>
/// <param name="CreatedAt">When the story was created.</param>
/// <param name="UpdatedAt">When the story was last updated.</param>
public record StoryDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUsername,
    string Title,
    string Description,
    string? CoverImageUrl,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
