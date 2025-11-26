using backend.Data;

namespace backend.Repositories;

public class PostRepository(DataContext context): 
    BaseRepository<backend.Models.Post>(context), backend.Interfaces.IPostRepository
{

}
