using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Post.Request;

public class CreatePostDTO
{
    [MinLength(1), MaxLength(255)]
    public required string Caption { get; set; }

    [MinLength(1), MaxLength(10000)]
    public required string Content { get; set; }

    public bool RegistredUsersOnly { get; set; } = false;
}
