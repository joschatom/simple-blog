using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;

/// <summary>
/// DTO for a query for a user or list of matching users.
/// </summary>
public class QueryUserDTO
{
    /// <summary>
    /// Id of the user.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Email of the user.
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Username of the account.
    /// </summary>
    [Username]
    public string? Username { get; set; }

    /// <summary>
    /// ID of a post that user posted.
    /// </summary>
    public Guid? PostId { get; set; }
}
