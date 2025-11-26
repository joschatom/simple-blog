using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Post.Request;

public class UpdatePostDTO
{
    [MinLength(1), MaxLength(255)]
    public string? Caption { get; set; }

    [MinLength(1)]
    public string? Content { get; set; }

    public bool? RegistredUsersOnly { get; set; } = false;
}