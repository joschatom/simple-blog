using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Post.Request;

/// <summary>
/// Request DTO for creating a new user (aka. Register)
/// </summary>
public class CreatePostDTO
{
    /// <summary>
    /// The caption of post.
    /// </summary>
    /// <example>This is my blogpost!</example>
    [MinLength(1), MaxLength(255), Required]
    public required string Caption { get; set; }

    /// <summary>
    /// The content of the post, currently just text.
    /// </summary>
    /// <example> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc nisi...</example>
    [MinLength(1), MaxLength(10000), Required]
    public required string Content { get; set; }
    /// <summary>
    /// Flag if the post can only be seen by logged in users.
    /// </summary>
    [DefaultValue(false)]
    public bool RegistredUsersOnly { get; set; } = false;
}