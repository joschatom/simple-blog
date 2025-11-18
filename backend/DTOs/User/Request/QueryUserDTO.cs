namespace backend.DTOs.User.Request;

public class QueryUserDTO
{
    public Guid? Id { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public Guid? PostId { get; set; }
}
