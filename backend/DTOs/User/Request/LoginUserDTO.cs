using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace backend.DTOs.User.Request;

public class LoginUserDTO
{
    [MinLength(1), MaxLength(255)]
    public required string Username { get; set; }

    [MinLength(6), MaxLength(255)]
    public required string Password { get; set; }
}
