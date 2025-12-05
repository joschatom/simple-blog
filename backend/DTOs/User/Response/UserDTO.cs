using AutoMapper;
using backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace backend.DTOs.User.Response;

/// <summary>
/// DTO for user data, can also contain private data.
/// </summary>
/// <remarks>
/// This should only be sent to the client where that user is logged in, NO OTHER CLIENTS.
/// </remarks>
public class UserDTO : IEquatable<Models.User>
{
    /// <summary>
    /// The Unique Identifier of the user.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Name of the user (aka. username).
    /// </summary>
    /// <example>testuser</example>
    public required string Username { get; set; }
    /// <summary>
    /// Email Address of the user.
    /// </summary>
    /// <example>test@example.com</example>
    public required string Email { get; set; }
    /// <summary>
    /// When the account was created.
    /// </summary>
    public required DateTime CreatedAt { get; set; }
    /// <summary>
    /// When the account's info was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public bool Equals(Models.User? other)
        => Id == other?.Id
        && Username == other.Username
        && Email == other.Email
        && CreatedAt == other.CreatedAt
        && UpdatedAt == other.UpdatedAt;

}

