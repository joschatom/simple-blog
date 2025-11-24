using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Response;

public class PublicUserDTO
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required DateTime CreatedAt { get; set; }
}
