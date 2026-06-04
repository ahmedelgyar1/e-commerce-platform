using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Domain.Interfaces;

namespace e_commerce_platform.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext Context;

    public BaseRepository(AppDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await Context.Set<T>().FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
    }

    public void Update(T entity)
    {
        Context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync() > 0;
    }
}
