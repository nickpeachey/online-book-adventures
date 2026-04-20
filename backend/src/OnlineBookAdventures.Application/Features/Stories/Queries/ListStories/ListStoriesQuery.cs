using MediatR;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.ListStories;

/// <summary>
/// Query to retrieve a paginated list of published stories.
/// </summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of results per page.</param>
/// <param name="SearchTerm">An optional search term to filter by title or description.</param>
public record ListStoriesQuery(int Page = 1, int PageSize = 20, string? SearchTerm = null) : IRequest<ListStoriesResult>;

/// <summary>
/// Paginated list of stories.
/// </summary>
/// <param name="Stories">The stories on the current page.</param>
/// <param name="TotalCount">The total number of matching stories.</param>
/// <param name="Page">The current page number.</param>
/// <param name="PageSize">The page size.</param>
public record ListStoriesResult(IReadOnlyList<StoryDto> Stories, int TotalCount, int Page, int PageSize);
