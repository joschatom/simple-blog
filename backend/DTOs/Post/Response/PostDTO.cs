using AutoMapper.Configuration.Annotations;
using backend.Models;

namespace backend.DTOs.Post.Response;

public class PostDTO
{
    Guid Id { get; set; }

    public required string Caption { get; set; }
    public required string Content { get; set; }

    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    [SourceMember(nameof(Models.Post.UserId))]
    public Guid AuthorId { get; set; }

    public bool RegistredUsersOnly { get; set; }
}
