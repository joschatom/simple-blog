namespace backend.DTOs.User.Response;

public class AuthUserDTO: UserDTO
{
    public required string Token { get; set; }  
}
