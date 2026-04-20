using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Story> Stories { get; }
    DbSet<StoryNode> StoryNodes { get; }
    DbSet<Choice> Choices { get; }
    DbSet<Progress> Progresses { get; }
    DbSet<Rating> Ratings { get; }
    DbSet<Comment> Comments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
