using AutoMapper;
using backend.DTOs.Post.Response;
using backend.DTOs.Shared.Response;
using backend.DTOs.User.Request;
using backend.DTOs.User.Response;
using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using backend.Tests.Posts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
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

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdatedDTO>> UpdateUser(Guid id, UpdateUserDTO update)
    {
        if ((await this.CurrentUser())!.Id != id) // TODO: Admins
            return Unauthorized($"Cannot update someone else's account yet.");

        var user = await repository.GetByIdAsync(id);

        if (user is null)
            return NotFound($"User with ID {id} cannot be found.");

        UpdatedDTOBuilder<UpdateUserDTO> builder = new();

        if (update.Username is not null)
        {
            if (repository.GetByNameAsync(update.Username) is not null)
                return Problem(
                    title: "Username already taken",
                    detail: $"Cannot update username to {update.Username} as it is already taken.",
                    statusCode: StatusCodes.Status400BadRequest
                );

            builder.AddFields(nameof(UpdateUserDTO.Username));
            user.Username = update.Username;
        }
        if (update.Email is not null)
        {
            builder.AddFields(nameof(UpdateUserDTO.Username));
            user.Email = update.Email;
        }

        await repository.UpdateAsync(id, user);
        await repository.SaveChangesAsync();

        return Ok(builder.Build());
    }

    [HttpPost("{id}/mute")]
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

    [AllowAnonymous]
    [HttpGet("{id}/posts")]
    public async Task<ActionResult<PostDTO>> GetPosts(Guid id)
    {
        if (!await repository.ExistsAsync(id))
            return NotFound($"Post with {id} not found.");

        var users = (await repository.GetPosts(id))
            .Where(p => this.CurrentUser() is not null || !p.RegistredUsersOnly)
            .ToList();

        return Ok(mapper.Map<IEnumerable<PostDTO>>(users));
    }
}


