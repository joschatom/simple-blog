using backend.Models;

namespace backend.Interfaces;

public interface IUserRepository: IBaseRepository<User>
{
    public Task<User?> GetByNameAsync(string username);
    public Task<User?> GetByEmailAsync(string email);

    public Task MuteUser(Guid muter, Guid mutee);


}
