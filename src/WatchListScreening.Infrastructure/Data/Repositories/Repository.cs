using Microsoft.EntityFrameworkCore;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Common;

namespace WatchListScreening.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }
    public void Update(T entity)
        => _dbSet.Update(entity);
    public void Delete(T entity)
        => _dbSet.Remove(entity);
    public IQueryable<T> Query()
        => _dbSet.AsQueryable();
}
