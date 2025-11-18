using backend.Data;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class BaseRepository<T>(DataContext context) : IBaseRepository<T>
    where T : class
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T> CreateAsync(T entity)
        => (await _dbSet.AddAsync(entity)).Entity;

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null)
            return false;
        _dbSet.Remove(entity);
        return true;
    }

    public Task<bool> ExistsAsync(Guid id)
        => GetByIdAsync(id).ContinueWith(task => task.Result is not null);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public async Task<bool> SaveChangesAsync()
        => await context.SaveChangesAsync() != 0;

    public async Task<T?> UpdateAsync(Guid id, T entity)
    {
        if (!await ExistsAsync(id))
            return null;

        return _dbSet.Update(entity).Entity;
    }
}
