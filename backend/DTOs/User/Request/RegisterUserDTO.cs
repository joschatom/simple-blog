using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;

public class RegisterUserDTO
{
    [MinLength(1), MaxLength(255)]
    public required string Username { get; set; }
    [EmailAddress, MaxLength(255)]
    public required string Email { get; set; }
    [MinLength(6), MaxLength(255)]
    public required string Password { get; set; }
}
