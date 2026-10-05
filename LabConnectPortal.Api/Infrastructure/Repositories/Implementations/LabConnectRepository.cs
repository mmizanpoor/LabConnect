using System.Linq.Expressions;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class LabConnectRepository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;
    private readonly LabConnectDbContext _context;

    public LabConnectRepository(LabConnectDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(object id)
        => await _dbSet.FindAsync(id);

    public void Add(T model) => _dbSet.Add(model);

    public void Update(T model) => _dbSet.Update(model);

    public void Delete(T model) => _dbSet.Remove(model);

    public async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return OperationResult.Failure(message);
        }
    }
}
