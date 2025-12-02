using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;
public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly DBContext _context;

    public BaseRepository(DBContext context)
    {
        _context = context;
    }
    
    public async Task<T> CreateAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public async Task<T> DeleteAsync(int Id)
    {
        var entity = await FindByIdAsync(Id);

        if (entity == null)
            throw new Exception($"Entity with id {Id} not found");

        _context.Set<T>().Remove(entity);
        return entity;
    }

    public async Task<T> DeleteAsync(string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Id cannot be null or empty", nameof(Id));
        var entity = await FindByIdAsync(int.Parse(Id));
        if (entity == null)
            throw new Exception($"Entity with id {Id} not found");
        _context.Set<T>().Remove(entity);
        return entity;
    }

    public IQueryable<T> FindAll()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();
            
    }

    public async Task<T?> FindByIdAsync(int Id)
    {
        return await _context.Set<T>().FindAsync(Id);

    }

    public async Task<T> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        //await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<int> UpdateRangeAsync(List<T> entities)
    {
        if (entities == null || !entities.Any())
            return 0;

        _context.Set<T>().UpdateRange(entities);
        return await _context.SaveChangesAsync();


    }
}
