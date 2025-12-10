using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class PostRepository(DataContext context) :
    BaseRepository<backend.Models.Post>(context), backend.Interfaces.IPostRepository
{
    private readonly DataContext context = context;
    public async Task DeleteAllPosts(Guid userId)
    {
        await context.Posts
            .Where(u => u.UserId == userId)
            .ExecuteDeleteAsync();
    }
}
