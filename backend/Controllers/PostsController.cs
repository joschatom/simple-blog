using AutoMapper;
using backend.Data;
using backend.DTOs.Post.Request;
using backend.DTOs.Post.Response;
using backend.DTOs.Shared.Response;
using backend.DTOs.User.Response;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            .OrderByDescending(k => k.CreatedAt)
         );
            
    }
    
    /// <summary>
    /// Endpoint to get the post with the given id.
    /// </summary>
    /// <param name="id">The ID of the post.</param>
    /// <returns>If the post exist return the post data and if not an error.</returns>
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

    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="createPost">Information about the post to create.</param>
    /// <returns>If sucessful the data for the newly created Post (including the ID).</returns>
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

    /// <summary>
    /// Updates a post.
    /// </summary>
    /// <param name="id">The ID of the post to update</param>
    /// <param name="update">New values for field or null if not updated (per field).</param>
    /// <returns>If successful returns the fields that where updated.</returns>
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

    /// <summary>
    /// Deletes all post created by the current user.
    /// </summary>
    /// <returns>If successful an HTTP 200 and an empty body.</returns>
    // DELETE api/posts/1A577ADE-B695-406A-83ED-FA161EDD02A9
    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAllPosts()
    {
        var user = this.GetCurrentUserId();

        if (!await userRepository.ExistsAsync(user))
            return BadRequest("User given in token doesn't exist.");

        await repository.DeleteAllPosts(user);
        await repository.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Delete a post.
    /// </summary>
    /// <param name="id">ID of the post to delete.</param>
    /// <returns>HTTP 200 if successfull.</returns>
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
