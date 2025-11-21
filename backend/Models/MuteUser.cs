using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace backend.Models;

/// <summary>
/// Model representing a muted user relationship between two users.
/// </summary>
[PrimaryKey("UserId", "MutedUserId")]
public class MuteUser
{
    // === Composite Primary Key ===

    public Guid UserId { get; set; }
    public Guid MutedUserId { get; set; }

    // === Navigation Properties ===

    public virtual User User { get; set; } = null!;
    public virtual User MutedUser { get; set; } = null!;
}
