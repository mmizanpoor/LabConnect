using System.Linq.Expressions;
using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    void Add(T model);
    void Update(T model);
    void Delete(T model);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(object id);
    Task<OperationResult> SaveChangesAsync();
}
