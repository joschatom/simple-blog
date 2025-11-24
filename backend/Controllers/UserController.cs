using AutoMapper;
using backend.DTOs.User.Response;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.RegularExpressions;

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


    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
        => mapper.Map<UserDTO>(await this.CurrentUser());

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

        if ((await this.CurrentUser())!.Id != id) // TODO: Admins
            return Unauthorized($"Cannot delete someone else's account.");

        await repository.DeleteAsync(id);
        await repository.SaveChangesAsync();

        return SignOut();
    }


    [HttpDelete("{id}/mute")]
    public async Task<ActionResult> MuteUser(Guid id)
    {
        if (!await repository.ExistsAsync(id))
            return NotFound($"User with ID {id} cannot be found.");

        var currentUser = await this.CurrentUser();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await repository.MuteUser(currentUser!.Id, id);
        await repository.SaveChangesAsync();

        return Ok();
    }
}


