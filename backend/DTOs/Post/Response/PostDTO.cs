using AutoMapper.Configuration.Annotations;
using backend.DTOs.User.Response;
using backend.Models;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text.Json.Serialization;

namespace backend.DTOs.Post.Response;

/// <summary>
/// Primary DTO for representing a Post.
/// </summary>
public class PostDTO
{
    /// <summary>
    /// Unique Identifier for that post.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Caption for the post.
    /// </summary>
    /// <example>In egestas bibendum felis a euismod.</example>
    public required string Caption { get; set; }
    /// <summary>
    /// The Content/text of the Post body.
    /// </summary>
    /// <example>auris malesuada mauris at ante consectetur...</example>
    public required string Content { get; set; }

    /// <summary>
    /// When the post was initally created/posted.
    /// </summary>
    public required DateTime CreatedAt { get; set; }
    /// <summary>
    /// When the post was last updated at.
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The ID of the user who created the post.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Public data about the author of the post (should have same ID as <see cref="UserId"/>).
    /// </summary>
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PublicUserDTO User { get; set; } = null!;

    /// <summary>
    /// If the post is only visible for logged in users.
    /// </summary>
    public bool RegistredUsersOnly { get; set; }

    /// <summary>
    /// <seealso cref="object.ToString"/> override for a nicer repsentation of the Post data.
    /// </summary>
    /// <returns>A string representing the post in a readable nicely formatted way.</returns>
    public override string ToString()
    {
        return
           $"""
            PostDTO {"{"}
                Id = {Id}, Caption = {Caption}
                Content = {Content.Split("\n").First()}
                CreatedAt = {CreatedAt}"
                UpdatedAt = {UpdatedAt}, UserId = {UserId}, 
                RegistredUsersOnly = {RegistredUsersOnly}
            {"}"}
            """;

    }
}
