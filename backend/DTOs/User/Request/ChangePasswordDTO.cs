using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;

/// <summary>
/// DTO for a request to change a user's password.
/// </summary>
public class ChangePasswordDTO
{
    /// <summary>
    /// New Password for user.
    /// </summary>
    /// <example>mypassword123</example>
    [Required, MinLength(Models.User.MIN_PASSWORD_LENGTH), MaxLength(255)]
    public required string Password { get; set; }
}
