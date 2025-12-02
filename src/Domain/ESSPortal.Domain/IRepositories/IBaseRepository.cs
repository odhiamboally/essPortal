using System.Linq.Expressions;

namespace ESSPortal.Domain.IRepositories;
public interface IBaseRepository<T> where T : class
{
    Task<T> CreateAsync(T entity);
    Task<T> DeleteAsync(int Id);
    Task<T> DeleteAsync(string Id);
    IQueryable<T> FindAll();
    Task<T?> FindByIdAsync(int Id);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    Task<T> UpdateAsync(T entity);
    Task<int> UpdateRangeAsync(List<T> entities);

}
