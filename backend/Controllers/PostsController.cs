using AutoMapper;
using backend.Data;
using backend.DTOs.Post.Request;
using backend.DTOs.Post.Response;
using backend.DTOs.Shared.Response;
using backend.DTOs.User.Response;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Constraints;
using System.ComponentModel;
using System.Diagnostics;


namespace backend.Controllers;

[Route("api/posts")]
[Authorize]
[ApiController]
public class PostsController(DataContext context, IPostRepository repository, IUserRepository userRepository, IMapper mapper) : ControllerBase
{
    // GET: api/posts
    /// <summary>
    /// Get a list of all posts or
    /// posts where <see cref="Models.Post.RegistredUsersOnly"/> is set to <see langword="false"/> if logged out.
    /// </summary>
    /// <returns>A list of all posts as json</returns> 
    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<PostDTO>> GetAll()
    {
        var currentUser = await this.CurrentUser();

        return mapper.Map<IEnumerable<PostDTO>>(
            (await repository.GetAllAsync())
            .Where((post) => (currentUser is not null || !post.RegistredUsersOnly))
            .Where((post) => currentUser is not null 
                ? !userRepository.IsMutedBlocking(currentUser!, post.UserId)
                : true
            )
            .OrderBy(k => k.CreatedAt)
         );
            
    }

    // GET api/posts/1A577ADE-B695-406A-83ED-FA161EDD02A9
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostDTO>> GetPostById(Guid id)
    {
        var post = await repository.GetByIdAsync(id);

        if (post is null)
            return NotFound($"Post with ID {id} cannot be found.");

        if (post.RegistredUsersOnly && (await this.CurrentUser()) is null)
            return Forbid(); // maybe Unauthorized() or just NotFound() is better?

        var map = mapper.Map<PostDTO>(post);

        map.User = mapper.Map<PublicUserDTO>(post.User);
        
        return map;
    }

    // POST api/posts
    [HttpPost]
    public async Task<ActionResult<PostDTO>> Post([FromBody] CreatePostDTO createPost)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var postEntity = mapper.Map<backend.Models.Post>(createPost);
        postEntity.UserId = this.GetCurrentUserId(); // <-- this was missing...
        postEntity.User = await this.CurrentUser();
        
        var createdPost = await repository.CreateAsync(postEntity);
        await repository.SaveChangesAsync();

        var postToReturn = mapper.Map<PostDTO>(createdPost);
        return CreatedAtAction(nameof(GetPostById), new { id = postToReturn.Id }, postToReturn);
    }

    // PUT api/posts/1A577ADE-B695-406A-83ED-FA161EDD02A9
    [HttpPut("{id}")]
    public async Task<ActionResult<UpdatedDTO>> Put(Guid id, [FromBody] UpdatePostDTO update)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var builder = new UpdatedDTOBuilder<UpdatePostDTO>();

        var existingPost = await repository.GetByIdAsync(id);


        if (existingPost is null)
            return NotFound($"Post with ID {id} cannot be found.");
        else if (existingPost.UserId != this.GetCurrentUserId())
            return Forbid();

        if (update.Caption is not null)
        {
            existingPost.Caption = update.Caption;
            builder.AddFields(nameof(update.Caption));
        }
        if (update.Content is not null)
        {
            existingPost.Content = update.Content;
            builder.AddFields(nameof(update.Content));
        }
        if (update.RegisteredUsersOnly is not null)
        {
            existingPost.RegistredUsersOnly = update.RegisteredUsersOnly.Value;
            builder.AddFields(nameof(update.RegisteredUsersOnly));
        }

        Console.WriteLine(existingPost.ToString());

        await repository.UpdateAsync(id, existingPost);
        await repository.SaveChangesAsync();

        return Ok(builder.Build());
    }

    // DELETE api/posts/1A577ADE-B695-406A-83ED-FA161EDD02A9
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingPost = repository.GetByIdAsync(id).Result;

        if (existingPost is null)
            return NotFound($"Post with ID {id} cannot be found.");
        else if (existingPost.UserId != this.GetCurrentUserId())
            return Forbid();

        await repository.DeleteAsync(id);
        await repository.SaveChangesAsync();

        return Ok(new { success = true });
    }
}
