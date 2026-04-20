namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a registered user of the platform.
/// </summary>
public class User : BaseEntity
{
    /// <summary>Gets or sets the user's unique username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCrypt-hashed password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time of the user's last login.</summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the stories authored by this user.</summary>
    public ICollection<Story> AuthoredStories { get; set; } = [];

    /// <summary>Gets or sets the reading progress records for this user.</summary>
    public ICollection<Progress> Progresses { get; set; } = [];

    /// <summary>Gets or sets the ratings submitted by this user.</summary>
    public ICollection<Rating> Ratings { get; set; } = [];

    /// <summary>Gets or sets the comments posted by this user.</summary>
    public ICollection<Comment> Comments { get; set; } = [];
}
