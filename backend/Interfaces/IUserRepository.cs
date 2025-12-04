using backend.Models;

namespace backend.Interfaces;

public interface IUserRepository: IBaseRepository<User>
{
    public Task<User?> GetByNameAsync(string username);
    public Task<User?> GetByEmailAsync(string email);
    public Task<IEnumerable<Post>> GetPosts(Guid id);

    public Task MuteUser(Guid muter, Guid mutee);
    public Task<bool> UnmuteUser(Guid muter, Guid mutee);

    /// <summary>
    /// Gets a list of all users muted by this user.
    /// </summary>
    /// <param name="id">The ID of the user to query list for.</param>
    /// <returns>A list of object describing the "muted" relationship.</returns>
    public Task<IEnumerable<MuteUser>> GetMutedUsers(Guid id);

}
