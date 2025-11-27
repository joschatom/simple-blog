using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;


public class UpdateUserDTO
{
    [Username]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
