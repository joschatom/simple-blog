using AutoMapper;
using backend.DTOs.Post.Request;
using backend.DTOs.Post.Response;
using backend.DTOs.Shared.Response;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace backend.Controllers;

[Route("api/posts")]
[Authorize]
[ApiController]
public class PostsController(IPostRepository repository, IMapper mapper) : ControllerBase
{
    // GET: api/posts
    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<PostDTO>> GetAll()
    {
        var currentUser = await this.CurrentUser();

        return mapper.Map<IEnumerable<PostDTO>>(
            (await repository.GetAllAsync())
            .Where((post) => (currentUser is not null || !post.RegistredUsersOnly))
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

        return mapper.Map<PostDTO>(post);
    }

    // POST api/posts
    [HttpPost]
    public async Task<ActionResult<PostDTO>> Post([FromBody] CreatePostDTO createPost)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var postEntity = mapper.Map<backend.Models.Post>(createPost);
        postEntity.UserId = this.GetCurrentUserId(); // <-- this was missing...

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
        if (update.RegistredUsersOnly is not null)
        {
            existingPost.RegistredUsersOnly = update.RegistredUsersOnly.Value;
            builder.AddFields(nameof(update.RegistredUsersOnly));
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
