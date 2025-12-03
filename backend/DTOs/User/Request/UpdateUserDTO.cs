using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;

/// <summary>
/// DTO for a request to update a user's information.
/// </summary>
public class UpdateUserDTO
{
    /// <summary>
    /// New username or null if NOT updated.
    /// </summary>
    /// <example>my-new-username</example>
    /// 
    [Username]
    public string? Username { get; set; }

    /// <summary>
    /// New email address or null if NOT updated.
    /// </summary>
    /// <example>me@new.example.com</example>
    
    [EmailAddress]
    public string? Email { get; set; }
}
