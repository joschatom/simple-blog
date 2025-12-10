using backend.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace backend.DTOs.User.Request;

/// <summary>
/// DTO for a login request.
/// </summary>
public class LoginUserDTO
{
    /// <summary>
    /// The username of the account to attempt to log into.
    /// </summary>
    /// <example>testuser</example>
    [Username, Required]
    public required string Username { get; set; }

    /// <summary>
    /// The password of that account.
    /// </summary>
    /// <example>testpassword</example>
    [MinLength(Models.User.MIN_PASSWORD_LENGTH), MaxLength(255), Required]
    public required string Password { get; set; }
}
