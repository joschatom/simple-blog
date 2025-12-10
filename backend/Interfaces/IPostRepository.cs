using backend.Tests.Posts;

namespace backend.Interfaces;

public interface IPostRepository: IBaseRepository<backend.Models.Post>
{
    Task DeleteAllPosts(Guid userId); 
}
