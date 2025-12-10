using backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace backend.DTOs.User.Request;

/// <summary>
/// DTO for a request to register a new account.
/// </summary>
public class RegisterUserDTO
{
    /// <summary>
    /// Username of the new account.
    /// </summary>
    /// <example>myaccount124</example>
    [Username, Required] 
    public required string Username { get; set; }

    /// <summary>
    /// Email for the new account. 
    /// </summary>
    /// <example>me@example.com</example>
    [EmailAddress, MaxLength(255), Required]
    public required string Email { get; set; }

    /// <summary>
    /// The password for the new account.
    /// </summary>
    /// <example>mypassword</example>
    [MinLength(Models.User.MIN_PASSWORD_LENGTH), MaxLength(255), Required]
    public required string Password { get; set; }
}
