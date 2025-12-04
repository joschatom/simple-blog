using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace backend.Models;

[DebuggerDisplay("Post: {Caption} (Id: {Id})")]

public class Post: TimedModel
{
    // === Primary Key ===
    public Guid Id { get; set; }

    // === Post Data ===
    public required string Caption { get; set; }
    public required string Content { get; set; } = string.Empty;

    // === Metadata ===
    public required Guid UserId { get; set; }
    public virtual User? User { get; set; } = null!;
    public bool RegistredUsersOnly { get; set; } = false;

    public override string ToString()
    {
        return $"<Post titled \"{Caption}\" (ID {Id})>";
    }

}
