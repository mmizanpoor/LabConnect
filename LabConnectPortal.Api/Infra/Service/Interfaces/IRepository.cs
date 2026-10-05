using System.Linq.Expressions;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T model);
        void Update(T model);
        void Delete(T model);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> predicate);
        Task<T?> GetById(object id);
        Task<OperationResult> SaveChange();
    }
}
