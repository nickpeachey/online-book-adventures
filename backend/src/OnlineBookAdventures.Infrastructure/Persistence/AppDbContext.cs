using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Infrastructure.Persistence;

/// <summary>
/// Main application database context.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    /// <summary>Gets or sets the users table.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Gets or sets the stories table.</summary>
    public DbSet<Story> Stories => Set<Story>();

    /// <summary>Gets or sets the story nodes table.</summary>
    public DbSet<StoryNode> StoryNodes => Set<StoryNode>();

    /// <summary>Gets or sets the choices table.</summary>
    public DbSet<Choice> Choices => Set<Choice>();

    /// <summary>Gets or sets the reader progress table.</summary>
    public DbSet<Progress> Progresses => Set<Progress>();

    /// <summary>Gets or sets the ratings table.</summary>
    public DbSet<Rating> Ratings => Set<Rating>();

    /// <summary>Gets or sets the comments table.</summary>
    public DbSet<Comment> Comments => Set<Comment>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
