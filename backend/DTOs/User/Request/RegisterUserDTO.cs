using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace backend.DTOs.User.Request;

public class RegisterUserDTO
{
    [MinLength(1), MaxLength(255)]
    [RegularExpression(@"^[a-zA-Z0-9-]+$", 
        ErrorMessage = "Usernames can only contain a-z, A-Z, 0-9, and -")] 
    public required string Username { get; set; }
    [EmailAddress, MaxLength(255)]
    public required string Email { get; set; }
    [MinLength(6), MaxLength(255)]
    public required string Password { get; set; }
}
