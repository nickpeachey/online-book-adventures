namespace OnlineBookAdventures.Domain.Entities;

/// <summary>
/// Represents a user's rating of a story on a 1–5 scale.
/// </summary>
public class Rating : BaseEntity
{
    /// <summary>Gets or sets the identifier of the user who submitted this rating.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the identifier of the rated story.</summary>
    public Guid StoryId { get; set; }

    /// <summary>Gets or sets the rating score from 1 (lowest) to 5 (highest).</summary>
    public int Score { get; set; }

    // Navigation properties
    /// <summary>Gets or sets the user who submitted this rating.</summary>
    public User User { get; set; } = null!;

    /// <summary>Gets or sets the rated story.</summary>
    public Story Story { get; set; } = null!;
}
