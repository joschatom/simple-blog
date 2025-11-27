using backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace backend.DTOs.User.Request;

public class RegisterUserDTO
{
    [Username] 
    public required string Username { get; set; }

    [EmailAddress, MaxLength(255)]
    public required string Email { get; set; }

    [MinLength(6), MaxLength(255)]

    public required string Password { get; set; }
}
