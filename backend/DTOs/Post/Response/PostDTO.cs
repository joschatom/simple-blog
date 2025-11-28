using AutoMapper.Configuration.Annotations;
using backend.Models;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace backend.DTOs.Post.Response;

public class PostDTO
{
    public Guid Id { get; set; }

    public required string Caption { get; set; }
    public required string Content { get; set; }

    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    public Guid UserId { get; set; }

    public bool RegistredUsersOnly { get; set; }

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
