using backend.Models;

namespace backend.Interfaces;

public interface IUserRepository: IBaseRepository<User>
{
    public Task<User?> GetByNameAsync(string username);
    public Task<User?> GetByEmailAsync(string email);
    public Task<IEnumerable<Post>> GetPosts(Guid id);
    public Task MuteUser(Guid muter, Guid mutee);


}
