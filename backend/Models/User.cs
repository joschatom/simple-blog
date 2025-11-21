using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace backend.Models;

/// <summary>
/// Represents a user on the blogging platform.
/// </summary>
[DebuggerDisplay("User: {Username} (ID {Id})")]
[ModelBinder(BinderType = typeof(CurrentUserValueProvider))]
public class User: TimedModel
{
    // === Primary Key ===
    public Guid Id { get; set; }

    // === User Data ===
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    // public Role? Role { get; set; } = Role.User;

    // === Authentication Data ===
    public string? Token { get; set; }
    public DateTime? LastLogin { get; set; }

    // === Navigation Properties ===
    public virtual ICollection<Post> Posts { get; set; } = [];
    public virtual ICollection<MuteUser> MutedUsers { get; set; } = [];
}