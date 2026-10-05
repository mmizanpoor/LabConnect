using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> dbset;
        private readonly SamanehDbContext _context;

        public Repository(SamanehDbContext context)
        {
            _context = context;
            dbset = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await dbset.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> predicate)
        {
            return await dbset.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetById(object id)
        {
            return await dbset.FindAsync(id);
        }


        public void Add(T model)
        {
            dbset.Add(model);
        }

        public void Update(T model)
        {
            dbset.Update(model);
        }


        public void Delete(T model)
        {
            dbset.Remove(model);
        }

        public async Task<OperationResult> SaveChange()
        {
            try
            {
                await _context.SaveChangesAsync();
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
