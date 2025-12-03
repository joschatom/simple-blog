using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Post.Request;

/// <summary>
/// Request DTO for Updating a post.
/// </summary>
public class UpdatePostDTO
{
    /// <summary>
    /// The new title/caption of the post, or null if NOT updated.
    /// </summary>
    /// <example>In egestas bibendum felis a euismod.</example>
    [MinLength(1), MaxLength(255)]
    public string? Caption { get; set; }

    /// <summary>
    /// The new content of the blog, or null if not updated
    /// </summary>
    /// <example> Mauris malesuada mauris at ante consectetur, non dictum diam...</example>
    [MinLength(1)]
    public string? Content { get; set; }
    /// <summary>
    /// New value for if this post is only for logged in users.
    /// </summary>
    public bool? RegistredUsersOnly { get; set; }
}