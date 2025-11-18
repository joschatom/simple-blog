namespace backend.DTOs.User.Response;

public class UserDTO
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
