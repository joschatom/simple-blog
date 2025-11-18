using AutoMapper;
using backend.DTOs.User.Response;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("/api/users")]
public class UserController(IUserRepository repository, IMapper mapper)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<PublicUserDTO>> GetAllUsers()
        => mapper.Map<IEnumerable<PublicUserDTO>>(await repository.GetAllAsync());

    [NonAction]
    public async Task<User> CurrentUser() {
        var userData = User.FindFirstValue(ClaimTypes.UserData)
            ?? throw new Exception("Invalid Token");

        var id = JsonConvert.DeserializeObject<Guid>(userData!);

        return await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} cannot be found.");
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
        => mapper.Map<UserDTO>(await CurrentUser());

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<PublicUserDTO>> GetUserById(Guid id)
    {
        var user = await repository.GetByIdAsync(id);
        if (user is null)
            return NotFound($"User with ID {id} cannot be found.");
        return mapper.Map<PublicUserDTO>(user);
    }

    [AllowAnonymous]
    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<PublicUserDTO>> GetUserById(string name)
    {
        var user = await repository.GetByNameAsync(name);
        if (user is null)
            return NotFound($"User named {name} doesn't exist.");
        return mapper.Map<PublicUserDTO>(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        if (!await repository.ExistsAsync(id))
            return NotFound($"User with ID {id} cannot be found.");

        if ((await CurrentUser()).Id != id) // TODO: Admins
            return Unauthorized($"Cannot delete someone else's account.");

        await repository.DeleteAsync(id);
        await repository.SaveChangesAsync();

        return SignOut();
    }
}

