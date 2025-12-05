using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Swashbuckle.AspNetCore.Swagger;


namespace backend.DTOs.User.Response;

/// <summary>
/// DTO for public user data.
/// </summary>
/// <remarks>
/// Any information include here will be viewable by any USER and logged out clients.
/// </remarks>
public class PublicUserDTO
{
    /// <summary>
    /// An unique identifer for the user.
    /// </summary>
    public required Guid Id { get; set; }
    /// <summary>
    /// The name of the user (aka. username).
    /// </summary>
    /// <example>testuser</example>
    public required string Username { get; set; }
    
    /// <summary>
    /// When the account was created.
    /// </summary>
    public required DateTime CreatedAt { get; set; }
}
