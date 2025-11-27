using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace backend.Models;

/// <summary>
/// Represents a user on the blogging platform.
/// </summary>
[DebuggerDisplay("User: {Username} (ID {Id})")]
public partial class User: TimedModel
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


    [GeneratedRegex(@"^[a-zA-Z0-9-]+$")]
    public static partial Regex ValidUsername();
    public const string ValidUsernameRegex = @"^[a-zA-Z0-9-]+$";
}

// Valid Username.
public class UsernameAttribute : DataTypeAttribute
{
    public UsernameAttribute() : base(DataType.Text) { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is not string username) return false;

        if (!User.ValidUsername().IsMatch(username)) return false;

        if (username.Length == 0 && username.Length > 255) return false;

        return true;
    }
}